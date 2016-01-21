#!/bin/bash

L001="---"
L002="- hosts: _HOST_"
L003="  sudo: yes"
L004="  tasks:"
L005="    - name: package"
L006="      apt: name={{item}} state=_PACKAGE_STATE_ update_cache=true"
L007="      with_items:"
L008="      - _PACKAGE_"

COUNT=0
APT_COMMAND=
HOSTS=""
PROGRAMS=()

for VAR in "$@"; do
    COUNT=$((COUNT+1))

    if [[ $COUNT == 1 ]]; then
        APT_COMMAND="${VAR}"
        continue
    else
        if [[ "${HOSTS}" == "" ]]; then
            HOSTS="${VAR}"
        else
            PROGRAMS+=("${HOSTS}")
            HOSTS="${VAR}"
        fi
    fi
done

if [[ "${APT_COMMAND}" == "install" || "${APT_COMMAND}" == "remove" ]]; then

    if [[ "${APT_COMMAND}" == "install" ]]; then
        PACKAGE_STATE="present"
    elif [[ "${APT_COMMAND}" == "remove" ]]; then
        PACKAGE_STATE="absent"
    fi

    YML_PB=$(mktemp)

    echo "${L001}" >> "${YML_PB}"
    echo "${L002}" | sed "s/_HOST_/${HOSTS}/" >> "${YML_PB}"
    echo "${L003}" >> "${YML_PB}"
    echo "${L004}" >> "${YML_PB}"
    echo "${L005}" >> "${YML_PB}"
    echo "${L006}" | sed "s/_PACKAGE_STATE_/${PACKAGE_STATE}/" >> "${YML_PB}"
    echo "${L007}" >> "${YML_PB}"
    for PACKAGE in "${PROGRAMS[@]}"; do
        echo "${L008}" | sed "s/_PACKAGE_/${PACKAGE}/" >> "${YML_PB}"
    done

    echo "execute playbook:"
    cat "${YML_PB}"

    ansible-playbook "${YML_PB}" -s -K

    rm "${YML_PB}"
fi
