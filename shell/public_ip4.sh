#!/bin/bash

if [[ -z "${IFC_OUT}" ]]; then
    IFC_OUT=$(/sbin/ifconfig)
fi
echo "$IFC_OUT" | grep "^eth\|^venet" | awk '{print $1}' | \
    while read -r LINE ; do
            echo "$IFC_OUT" | grep "$LINE" -A 1 | grep "inet addr" | \
                    awk '{print $2}' | awk -F ':' '{print $2}' | \
                    sed 's/^127.*$//g' | uniq -1 | sed ' /^$/d'
    done | xargs
