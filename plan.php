<?php
include "auth.php";
if(!isAuthenticated($_POST["code"])) {
    echo '[["Die Veröffentlichung des Vertretungsplans ist leider nicht erlaubt:(","<a href=https://bid.lspb.de/explorer/Index/1046865/>Vertretungsplan herunterladen</a>",[]]]';
    return;
}
$plans = json_decode(file_get_contents("/home/bot/files/theo_plan"));

for ($i=0;$i<sizeof($plans);$i++)
{
    for ($j=0;$j<sizeof($plans[$i][2]);$j++)
    {
        if(sizeof($plans[$i][2][$j][0])>1) continue;

        if(isset($_GET["stufe"])&&sizeof($_GET["stufe"])==1&&$plans[$i][2][$j][0][0]==$_GET["stufe"]&&isset($_GET["Klasse"])&&strpos($plans[$i][2][$j][0], $_GET["Klasse"])!==false)
            $plans[$i][2][$j][0].="*";
        else if(sizeof($_GET)==1&&isset($_GET["stufe"])&&($plans[$i][2][$j][0]==$_GET["stufe"]||$plans[$i][2][$j][0][0]==$_GET["stufe"]))
            $plans[$i][2][$j][0].="*";
        else if(isset($_GET["stufe"])&&$plans[$i][2][$j][0]==$_GET["stufe"]&&in_array($plans[$i][2][$j][3], $_GET))
            $plans[$i][2][$j][0].="*";
    }
}

echo json_encode($plans);
