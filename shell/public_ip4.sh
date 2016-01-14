#!/bin/bash

if [[ -z "${IFC_OUT}" ]]; then
    IFC_OUT=$(/sbin/ifconfig)
fi

INET_ADDR=$(echo "$IFC_OUT" | grep "inet addr" | wc -l)
if [[ "${INET_ADDR}" > 0 ]]; then
    USE_COLON=1
else
    USE_COLON=0
fi

if [[ "${USE_COLON}" == 1 ]]; then
    echo "$IFC_OUT" | grep "^eth\|^venet\|^wlan" | awk '{print $1}' | \
        while read -r LINE ; do
                    echo "$IFC_OUT" | grep "$LINE" -A 1 | grep "inet addr" | \
                            awk '{print $2}' | awk -F ':' '{print $2}' | \
                            sed 's/^127.*$//g' | uniq -1 | sed ' /^$/d'
        done | xargs
else
    echo "$IFC_OUT" | grep "^eth\|^venet\|^wlan" | awk '{print $1}' | \
        while read -r LINE ; do
                    echo "$IFC_OUT" | grep "$LINE" -A 1 | grep "inet " | \
                            awk '{print $2}' | \
                            sed 's/^127.*$//g' | uniq -1 | sed ' /^$/d'
        done | xargs
fi
