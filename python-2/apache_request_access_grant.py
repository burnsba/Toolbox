#!/usr/bin/python

# first python script
# July 20, 2015
#
# The idea is to have apache "deny all" and then add ip addresses to the whitelist.
# This is automated with the script below. First a user has to request authorization by 
# visiting a specific url on the server, which will add an entry to the access log.
# This script will tail the log looking for entries which match the url. It makes a backup 
# of the log file, renames it to the current epoch time, and places it in the backup/ folder. It will then
# present the ip addresses and ask to change the log, adding entries "allow from [ip]"
# for each ip address. Above each is also a comment with the date when it was added
# and the useragent that requested the url. If the log file is changed, the apache2 service
# is then restarted.
#
# TODO: ignore ip addresses that have been blocked in the config
# TODO: restore config file on error

import os.path
import sys
import shutil
import time
import re
import subprocess
import datetime

class bcolors:
    CLEARALL = '\033[0m'
    BOLD = '\033[1m'
    UNDERLINE = '\033[4m'
    FG_RED = '\033[31m'
    FG_GREEN = '\033[32m'
    FG_BLUE = '\033[34m'
    BG_RED = '\033[41m'
    BG_GREEN = '\033[42m'
    BG_BLUE = '\033[44m'


class IpRequest:
    """Information about access log entry"""
    ip = ""
    identd = ""
    remoteUser = ""
    date = ""
    time = ""
    timezone = ""
    request = ""
    status = ""
    size = 0
    userAgent = ""
    
accessLogRegex = '(\S+) (\S+) (\S+) \[([^:]+):(\d+:\d+:\d+) ([^\]]+)\] \"(\S+) (.*?) (\S+)\" (\S+) (\S+) "([^"]*)" "([^"]*)"'
ipRegex = "([0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3})"
configFilePath = "/etc/apache2/sites-available/default"
logFilePath = "/var/log/apache2/access.log"
recentRequestCommand = "tail '" + logFilePath + "' | grep '/request_access'"

authorizedIps = []
requestingIps = []

configFileExists = os.path.isfile(configFilePath)


if (configFileExists):
    print "Found config file."
else:
    print bcolors.BG_RED + "Could not find config file" + bcolors.CLEARALL
    sys.exit()

currentTime = str(time.time())
backupPath = 'backup/' + currentTime
shutil.copy(configFilePath, backupPath)
backupExists = os.path.isfile(backupPath)
if (backupExists):
    print "Created config backup."
else:
    print bcolors.BG_RED + "Could not backup config file" + bcolors.CLEARALL
    sys.exit()

print "Parsing config file"

safety = 0

beginScriptSection = "# begin auto-script section {{{"
endScriptSection = "# end auto-script section }}}"
insideScriptSection = 0
lines = []
f = open(configFilePath, 'r')
for line in f:
    if re.search(beginScriptSection, line, re.I):
        safety += 1
        insideScriptSection = 1
        continue
    if re.search(endScriptSection, line, re.I):
        safety += 1
        insideScriptSection = 0

    if insideScriptSection == 1:
        lines.append(line.replace('\n', ''))
        ip = re.search('[Aa][lL][lL][oO][wW] [fF][rR][oO][mM] ' + ipRegex, line)
        if not (ip is None) and (ip.groups() > 1):
            ipToAdd = ip.group(1)

            if (ipToAdd in authorizedIps):
                print "Skipping duplicate ip for authorized list: " + ipToAdd
            else:
                print "Adding ip address to authorized list: " + ipToAdd
                authorizedIps.append(ip.group(1))

f.seek(0,0)
configLines = f.readlines()
f.close()

# print lines
# print authorizedIps

if not (safety == 2):
    print "Error parsing config file. Should have exactly one begin and end script section."
    sys.exit()

print "Parsing access log."

requests = subprocess.Popen(recentRequestCommand, shell=True, stdout=subprocess.PIPE).stdout.read()

requests = requests.split('\n')
requestEntries = []

for line in requests:
    acc = re.search(accessLogRegex, line)
    if not (acc is None) and \
            (acc.groups() > 12) and \
            not (acc.group(1) in requestingIps):
        ipEntry = IpRequest()
        ipEntry.ip = acc.group(1)
        ipEntry.userAgent = acc.group(13)
        ipEntry.date = acc.group(4)
        ipEntry.time = acc.group(5)
        ipEntry.timezone = acc.group(6)

        requestEntries.append(ipEntry)
        requestingIps.append(ipEntry.ip)

requestingIps[:] = [x for x in requestingIps if not (x in authorizedIps)]
requestEntries[:] = [x for x in requestEntries if not (x.ip in authorizedIps)]

if not (len(requestingIps) > 0):
    print "Could not find any new requests"
    sys.exit()

print "Requesting ips are as follows: "
print requestingIps

cont = raw_input("Allow ips [y/n]? ")

if not ((cont == 'y') or (cont == 'Y')):
    print "Exiting"
    sys.exit()

print "Adding ip address to apache config..."

f = open(configFilePath, 'r+b')
insertIndex = 1

while True:
    line = configLines[insertIndex]
    if re.search(endScriptSection, line, re.I):
        break
    insertIndex += 1

newContent = ""

for item in requestEntries:
    newLineComment = "# allowing ip '" + item.ip + "' on " + datetime.datetime.now().ctime() + ", request via " + item.userAgent + '\n'
    newLine = "Allow from " + item.ip + '\n'
    newContent += '\n' + newLineComment + newLine

#print "insert index: " + str(insertIndex)

configLines.insert(insertIndex, newContent)

configLines = "".join(configLines)
f.write(configLines)
f.close()

print "Restarting apache"

subprocess.Popen('service apache2 restart' , shell=True, stdout=subprocess.PIPE).stdout.read()
