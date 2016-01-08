#cs_go_rabbitmq_aspx_mysql

The web_client project has 3 parts:
1. web_client - a c# agent that pushes id and position to the Universe
2. Universe - a go program that takes care of the agents
3. AspxClient - an aspx client that shows the movements as they happen and more.

Until now the first 2 were implemented and the connection between Universe and AspxClient using RabbitMQ is also ready.

TODO:
Put the read data into a web page and into MySql
Read the data and show it in a timeline from MySql 
