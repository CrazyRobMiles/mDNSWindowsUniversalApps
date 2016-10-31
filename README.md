mDNS Manager
============

Helper object that makes it easy for Windows 10 Universal Applications to discover devices using mDNS (or Bonjour) for network discovery.

A system can find the hostname, ip address, socket and service name of a partiuclar service being advertised by devices using mDNS on a subnet.

Defaults to searching for tcp.http services, but you can search for others.

Events fired during the discovery process are passed back to the client. It is also possible for the client to abort a search and start a new one. 

Also includes a small sample program that can be used to display details of tcp http systems on the local network. 

Rob Miles
