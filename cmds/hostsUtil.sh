#!/bin/bash

# copy from https://gist.github.com/irazasyed/a7b0a079e7727a4315b9

# PATH TO YOUR HOSTS FILE
WIN_HOSTS=/c/Windows/System32/drivers/etc/hosts
WIN_WSL_HOSTS=/mnt/c/Windows/System32/drivers/etc/hosts
if [ -f $WIN_HOSTS ]
then
  # echo "Using windows hosts location"
  ETC_HOSTS=$UNIX_HOSTS
elif [ -f $WIN_WSL_HOSTS ]
then
  # echo "Using windows hosts location from WSL"
  ETC_HOSTS=$WIN_WSL_HOSTS
else
  # echo "Using unix hosts location"
  ETC_HOSTS=/etc/hosts
fi

# DEFAULT IP FOR HOSTNAME
IP="127.0.0.1"

# Hostname to add/remove.
HOSTNAME=$2

removehost() {
    echo "Removing $HOSTNAME";
    if [ -n "$(grep $HOSTNAME $ETC_HOSTS)" ]
    then
        echo " $HOSTNAME $HOSTNAME Found in your $ETC_HOSTS, Removing now...";
        sed -i".bak" "/$HOSTNAME/d" $ETC_HOSTS
    else
        echo " $HOSTNAME was not found in your $ETC_HOSTS";
    fi
    echo ""
    #make sure file is released before next update
    sleep 1
}

addhost() {
    echo "Adding $HOSTNAME";
    HOSTS_LINE="$IP\t$HOSTNAME"
    if [ -n "$(grep $HOSTNAME $ETC_HOSTS)" ]
        then
            echo -e " $HOSTNAME already exists \n $(grep $HOSTNAME $ETC_HOSTS)"
        else
            echo " Adding $HOSTNAME to your $ETC_HOSTS";
            echo "--$HOSTS_LINE--";
            sh -c -e "echo '$HOSTS_LINE' >> $ETC_HOSTS";

            if [ -n "$(grep $HOSTNAME $ETC_HOSTS)" ]
                then
                    echo -e " $HOSTNAME was added succesfully \n $(grep $HOSTNAME $ETC_HOSTS)";
                else
                    echo " Failed to Add $HOSTNAME, Try again!";
            fi
    fi
    echo ""
    #make sure file is released before next update
    sleep 1
}

printHosts() {
  echo "===    Hosts configuration    ==="
  echo -e  "$(cat $ETC_HOSTS)"
}

$@
