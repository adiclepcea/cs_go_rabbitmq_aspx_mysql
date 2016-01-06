package main

import (
	"encoding/json"
	"fmt"
	"log"
	"net/http"
	"sync/atomic"

	"github.com/streadway/amqp"
)

var A [100][100]uint32
var agentIndex uint32 = 0
var AChan chan [100][100]uint32

type Point struct {
	X int `json:"x"`
	Y int `json:"y"`
}

type AgentReq struct {
	Id uint32 `json:"id"`
	P  Point  `json:"point"`
}

func failOnError(err error, msg string) {
	if err != nil {
		log.Fatalf("%s: %s", msg, err)
		panic(fmt.Sprintf("%s: %s", msg, err))
	}
}

func main() {

	AChan = make(chan [100][100]uint32, 1)
	AChan <- A
	myServ := http.NewServeMux()
	myServ.HandleFunc("/", managerFunc)
	fmt.Println("Web server prepared")

	conn, err := amqp.Dial("amqp://guest:guest@localhost:5672/")
	failOnError(err, "Failed to connect to RabbitMQ")
	defer conn.Close()

	fmt.Println("Connected to RabbitMQ")
	ch, err := conn.Channel()
	failOnError(err, "Failed to open a channel")
	defer ch.Close()

	fmt.Println("RabbitMQ channel opened")

	log.Fatal(http.ListenAndServe(":8080", myServ))

}

func checkAgent(agent *AgentReq) {
	//Todo - check for inactivity to remove the id's not used
	//we could also use a buffered channel which would simplify
	//reuse of expired id's
	if agent.Id == 0 {
		index := atomic.AddUint32(&agentIndex, 1)
		agent.Id = index
	}

	//check if position is free, change it if not
	checkAgentPos(agent)
}

func checkAgentPos(agent *AgentReq) {
	arr, ok := <-AChan

	if !ok {
		log.Fatal("Could not get array from channel")
		agent.P.X = -1
		agent.P.Y = -1
		return
	}

	bEmptyFound := false
	bAgentFound := false

	if arr[agent.P.X][agent.P.Y] == 0 {
		arr[agent.P.X][agent.P.Y] = agent.Id
		bEmptyFound = true
	} else {
		//perhaps using a rand to specify the location would be more efficient
		//however, this one also works albeit you could get a lot of them on
		//the first row
		log.Printf("Agent %d moved from %d,%d to ", agent.Id, agent.P.X, agent.P.Y)
		bAgentFound = false
		for i := 0; i < 100; i++ {
			for j := 0; j < 100; j++ {
				if arr[i][j] == 0 {
					bEmptyFound = true
					arr[i][j] = agent.Id
					agent.P.X = i
					agent.P.Y = j
					break
				} else if arr[i][j] == agent.Id {
					arr[i][j] = 0
					bAgentFound = true
				}
			}
		}
	}

	if !bEmptyFound { //no more free places
		agent.P.X = -1
		agent.P.Y = -1
	} else if !bAgentFound { // we still have to find the agent and eliminate it's old pos
		for i := 0; i < 100; i++ {
			for j := 0; j < 100; j++ {
				if arr[i][j] == agent.Id && (i != agent.P.X || j != agent.P.Y) {
					arr[i][j] = 0
					bAgentFound = true
					break
				}
			}
		}
	}

	log.Printf("%d,%d\r\n ", agent.P.X, agent.P.Y)

	A = arr

	AChan <- A
}

//http server function
func managerFunc(w http.ResponseWriter, req *http.Request) {

	var agent AgentReq

	err := json.NewDecoder(req.Body).Decode(&agent)
	if err != nil {
		w.WriteHeader(404)
		w.Write([]byte("404 - Incorrect request!"))
		return
	}

	checkAgent(&agent)

	if err := json.NewEncoder(w).Encode(agent); err != nil {
		w.Write([]byte("Eroare"))
	}
}