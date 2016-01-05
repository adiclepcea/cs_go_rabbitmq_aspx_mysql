package main

import (
	"fmt"
	"net/http"
	"encoding/json"
)

type Point struct{
	X int `json:"x"`
	Y int `json:"y"` 
}

type AgentReq struct{
	Id int `json:"id"`
	P Point `json:"point"`
}

func main(){
	myServ := http.NewServeMux()
	myServ.HandleFunc("/",managerFunc)
	fmt.Println("Mucu cucu")
	http.ListenAndServe(":8080",myServ)
}

func managerFunc(w http.ResponseWriter, req *http.Request){
	
	var agent AgentReq//:= AgentReq{Id:0, X:10,Y:20}

	err := json.NewDecoder(req.Body).Decode(&agent)
	if err!=nil{
		w.WriteHeader(404)
		w.Write([]byte("404 - Incorrect request!"));
		return
	}
	//if agent.id==0 {
	//	agent.id=10;
	//}
	if err := json.NewEncoder(w).Encode(agent); err!=nil{
		w.Write([]byte("Eroare"));
	}
}


