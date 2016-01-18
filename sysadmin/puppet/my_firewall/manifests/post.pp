class my_firewall::post {

   firewall { "999 drop all other requests":
     action => "drop",
   }

 }
