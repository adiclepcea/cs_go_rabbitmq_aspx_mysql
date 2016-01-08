<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="main.aspx.cs" Inherits="AspxClient.WebForm1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <script src="jquery-2.1.4.min.js"></script>
</head>
<body>
    <style>
        body{
            font-family:Arial 'Franklin Gothic Medium', 'Arial Narrow', Arial, sans-serif;
            font-size:8pt;

        }
        table{
            border-spacing:0px;
            border-collapse:collapse;
        }
        td{
            border-color:black;
            border-width:1px;
            border-style:solid;
            padding:0px;
            width:13pt;
            height:15pt;            
        }
    </style>
    <!--I use this as plain HTML. I want to use plain AJAX for it-->
    <!--<form id="form1" runat="server">-->
    
    <button onclick="startStopShowData()" id="btnData">Show live data</button>
    <button onclick="startStopShowHistory()" id="btnHistory">Show history</button> &nbsp;
    <input type="text" id="seccondsBefore" value=""/> Seconds before
    <div id="msg1"></div>
    <div id="msg"></div>
    <div id="content">
        Mucu vesel
    </div>
   
        
    <!--</form>-->
    <script>
        var arrTemp = []; //this will know your location
        var showData = false;
        var showHistoryData = false;
        $(function () {
            var tbl = "<table id=\"tblMouvements\" cellpadding=0 cellspacing=0 witdh='130'>";
            for (var i = 0; i < 100; i++) {
                tbl += "<tr>";
                for (var j = 0; j < 100; j++) {
                    tbl += "<td><span id=\"s" + i + "_" + j + "\"></span></td>";
                    arrTemp[i * 100 + j] = "";
                }
                tbl += "</tr>";
            }
            tbl += "</table>";
            $("#content").html(tbl);
        });


        function startStopShowHistory() {
            var secsBefore = $("#seccondsBefore").val();
            if (!$.isNumeric(secsBefore)) {
                alert("Please choose a number!");
                $("#seccondsBefore").focus();
                return;
            }
            if (showHistoryData) {
                showHistoryData = false;
                $("#btnData").show();
                $("#btnHistory").text("Show history");
            } else {
                showHistoryData = true;
                $("#btnData").hide();
                $("#btnHistory").text("Stop showing history");
                showHistory(secsBefore);
            }

        }

        function showHistory(secsBefore){
            var d = new Date();
            var now = d.getTime();
            var nowMinusSecs = now - secsBefore * 1000 - d.getTimezoneOffset() * 60 * 1000;
            var strDate1 = new Date(nowMinusSecs).toISOString();
            nowMinusSecs = nowMinusSecs + 500;
            var strDate2 = new Date(nowMinusSecs).toISOString();            
            getHistoryData(strDate1, strDate2, secsBefore);
        }

        function getHistoryData(date1, date2, secsBefore) {
            $("#msg1").text("period " + date1 + " to " + date2);
            $.ajax({
                type: "POST",
                url: "./ajax.asmx/GetHistoryReadings",
                data: "data1="+date1+"&data2="+date2,
                dataType: "text",
                success: function (data) {
                    //$("#msg").text(data);
                    var obj = JSON.parse(data);
                    
                    displayData(obj);
                    
                },
                complete: function () {
                    if (showHistoryData) {
                        setTimeout(showHistory, 1000, secsBefore);
                    }
                },
                error: function () {
                    alert("error");
                }

            });
        }


        //show or stop showing the live data
        function startStopShowData() {
            if (showData == false) {                
                $("#btnData").text("Stop showing live data");
                showData = true;
                $("#btnHistory").hide();
                $("#seccondsBefore").hide();
                getData();
            } else {
                showData = false;
            }
        }

        //this one gets the live data from the server
        function getData() {
            //alert("sending");
            $.ajax({
                type: "POST",
                url: "./ajax.asmx/GetReadings",
                data: "",
                dataType: "json",
                success: function (data) {
                    var arr = data;
                    //alert(JSON.stringify(data));
                    displayData(arr);
                    
                },
                complete: function ()
                {
                    //we check if we need to show another piece of live data or not.
                    if (showData) {
                        setTimeout(getData(), 1000);//we do not use setInterval as we want to wait until completion and then call the function again
                    } else {
                        $("#btnData").text("Show live data");
                        $("#btnHistory").show();
                        $("#seccondsBefore").show();
                    }
                }
                

            });
            
        }
        //this one displays the data received from the server
        function displayData(arr) {
            //remove the old positions
            for (var i = 0; i < arr.length; i++) {
                var obj = arr[i];
                
                if (arrTemp[obj.id - 1] !== "" && arrTemp[obj.id - 1]!==undefined) {
                    var location = arrTemp[obj.id - 1].split(",");
                    $("#s" + location[0] + "_" + location[1]).text("");
                }
                
            }
            //show the new ones
            for (var i = 0; i < arr.length; i++) {
                var obj = arr[i];
                $("#s" + obj.point.x + "_" + obj.point.y).text(obj.id);
                arrTemp[obj.id - 1] = obj.point.x + "," + obj.point.y;
            }
        }


    </script>
</body>
    
</html>
