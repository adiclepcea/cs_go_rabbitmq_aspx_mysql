<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="main.aspx.cs" Inherits="AspxClient.WebForm1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <script src="jquery-2.1.4.min.js"></script>
</head>
<body>
    <form id="form1" runat="server">
    <div id="msg">
        Mucu vesel
    </div>
        <button onclick="getData()">OK</button>
    </form>
    <script>

        $();

        function getData() {
            
            $.ajax({
                type: "POST",
                url: "ajax.asmx/GetName",
                data: "",
                dataType: "text",
                success: function (data) {
                    alert(data);
                    console.log(data);
                }

            });
            
        }


    </script>
</body>
    
</html>
