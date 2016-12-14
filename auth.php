<?php
function isAuthenticated($p)
{
    if($p==null)
        return false;
    $id = $p??"";
    if(sizeof($id)!=24)
        return false;

    return strpos(file_get_contents("/home/bot/files/theo_codes"), $id) !== false;
}
