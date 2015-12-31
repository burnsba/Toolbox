disable auto indent for paste:

    :set paste

renable:

    :set nopaste

====================

current buffer filename: %

====================

run command:
    :!command

run command, output overwrite current line:
    :.!command

====================

ga (show character under cursor as ascii)

g8 (show character under cursor as utf8, including Unicode stuff, hex codes etc)

====================

new lines, carriage return:

Substituting by \n inserts a null character into the text. To get a newline, use \r. When searching for a newline, you’d still use \n, however.   

http://stackoverflow.com/questions/71323/how-to-replace-a-character-by-a-newline-in-vim

====================

increment number: CTRL+A
decrement number: CTRL+X

====================

registers: By the way, the same way "0 holds the last yank, "1 holds the last delete or change
The black hole register _ is the /dev/null of registers.

====================

case insensitive search, add "\c" somewhere

    /IgnoresCase\c
    /\cIgnoresCase

====================

replace first instance on current line:

    :s/old/new/

replace in entire file:

    :%s/old/new/g
    
====================

horizontal screen split:

    :sp
    
move between slpits:

    CTRL+w [direction]
