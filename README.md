#cs_go_rabbitmq_aspx_mysql

##The web_client project has 3 parts:

###1. web_client 
a CSharp agent that pushes id and position to the Universe

###2. Universe
a GO program that takes care of the agents

###3. AspxClient 
an aspx client that shows the movements as they happen and more.

### Usage
1. The first one to be started should be the GO program (Universe or Universe.exe). 
You can start whichever you want, but you will not get anything until the Universe 
is not up.

2. The clients (web_client) are working in Microsoft .NET and Mono. The tests however were used only in Microsoft .NET

3. The AspxClient uses AJAX and WebMethods


