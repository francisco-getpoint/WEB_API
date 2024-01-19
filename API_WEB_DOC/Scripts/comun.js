function go(u) {
    $(location).attr('href', u);
}
function goEXTERNAL(u) {
    $(location).attr({ "href": u, "target": "_blank" });
}
function goE(ajx, json, u) {
    $.ajax({ url: ajx, data: { JSON: JSON.stringify(json) }, cache: !1, type: "GET", timeout: 1e4, dataType: "json", success: function (i) { i.Success && $(location).attr("href", u) } });
}


function AJX_SET_NAVID(ID, URL, URL2, i, i2) {
    localStorage.setItem('TEMPDATA_NAVIGATION_DOCU', ID);
    goE(URL2, { 'Items': [{ 'Item1': i, 'Item2': i2 }] }, URL);

}
$(document).ready(function () {
    var ID = localStorage.getItem('TEMPDATA_NAVIGATION_DOCU'); 
    $("#LINK_" + ID).parent().parent().addClass("show");
    $("#LINK_" + ID).parent().addClass("show");
    $("#LINK_" + ID).html("<i class='fas fa-chevron-right'></i> " + $("#LINK_" + ID).html());
    $("#LINK_" + ID).addClass("text-primary");
});