class screen {
    package { 'screen':
        ensure => 'installed',
    }
}

class vim {
    package { 'vim':
        ensure => 'installed',
    }
}


class cron {
    package { 'cron':
        ensure => 'installed',
    }
}

class curl {
    package { 'curl':
        ensure => 'installed',
    }
}

node default {
}


node 'ip-172-31-46-190' {
    include my_firewall
    require screen
    require vim
    require cron
    require cron
}
