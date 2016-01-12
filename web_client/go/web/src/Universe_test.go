package main

import (

	"testing"

	)


func TestDeProba(t *testing.T){

	var A_test [100][100]uint32
	
	var agent_test AgentReq
	agent_test.Id = 1
	agent_test.P.X = 99
	agent_test.P.Y = 99
	
	//check if position is set ok in regular cases
	CheckAgentPos(&agent_test,&A_test)
	
	if A_test[99][99]==0 {
		t.Error("expected",1,"got",0)
	}
	
	//check if this one will get moved to a random pos 
	agent_test.Id = 2
	
	CheckAgentPos(&agent_test,&A_test)
	
	if A_test[99][99]!=1 {
		t.Error("expected",1,"got",2)
	}
	
	for i:=0; i<100;i++{
		for j:=0;j<100;j++{
			if A_test[i][j]==2 {
				t.Logf("i=%d; j=%d",i,j)
			}
		}
	}
	
	
	
	

}