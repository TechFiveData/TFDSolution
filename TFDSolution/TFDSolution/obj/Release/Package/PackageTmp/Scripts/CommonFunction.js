function formatBytes(bytes) {
    if (bytes === 0) return '0 Bytes';
    var k = 1024,
        sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB'],
        i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
}
function encodedLetterToNumber(number) {
    //Takes any number encoded with the provided encode dictionary 
    let result = '';
   //number = number - 1; // If starting from 1
    do {
        const letter = String.fromCharCode(65 + (number % 26));
        result = letter + result;
        number = Math.floor(number / 26) - 1;
    } while (number >= 0)
    return result;
}
function ShowToastNotification(type, message) {
    if (type == "success") {
        Command: toastr["success"](message)
    }
    else if (type == "warning") {
        Command: toastr["warning"](message)
    }
    else if (type == "error") {
        Command: toastr["error"](message)
    }
    else {
        Command: toastr["info"](message)
    }
}
function GetNextCode(tablename) {
    var code = "";
    $.ajax({
        type: "GET",
        url: '@Url.Action("getNextCode", "Common")',
        contentType: "application/json; charset=utf-8",
        data: { tblName: tablename },
        dataType: "json",
        success: function(response) {
            return response;
        },
        failure: function (response) {
            alert(response.d);
        },
        error: function (response) {
            alert(response);
        }
    });
    return code;
}
function SetDefaultSelection(did, ddlValue) {
    if (ddlValue != "" && ddlValue.length > 0 && ddlValue != "0") {
        $("#" + did).val(ddlValue).attr('selected', true);
    }
}
function convertNumber(txtdata) {
    if (txtdata != null && txtdata != undefined && txtdata != "") {
        return parseFloat(txtdata);
    }
    else {
        return 0;
    }
}
function formatDate(d) {
    if (d == undefined || d == "") {
        return "";
    }
    else if (hasTime(d)) {
        let n = d.toLocaleString([], {
            hour: "2-digit",    
            minute: "2-digit",
        });
        var s = d.getDate() + '/' + (d.getMonth() + 1) + '/' + d.getFullYear();
        /*s += ' ' + n;*/
    } else {
        var s = (d.getDate() + '/' + d.getMonth() + 1) + '/' + d.getFullYear();
    }
    return s;
}
function formatTimeAgo(dotNetDate) {

    // 🔹 Extract milliseconds
    var millis = parseInt(dotNetDate.replace(/[^0-9]/g, ''));

    var createdDate = new Date(millis);
    var now = new Date();

    var diffMs = now - createdDate; // difference in ms

    var diffMinutes = Math.floor(diffMs / (1000 * 60));
    var diffHours = Math.floor(diffMs / (1000 * 60 * 60));
    var diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24));

    // 🔥 Logic
    if (diffHours < 24) {
        var hours = diffHours;
        var minutes = diffMinutes % 60;

        return hours + "h " + minutes + "m";
    } else {
        return diffDays + "d";
    }
}
function formatDateTime(d) {
    if (d == undefined || d == "") {
        return "";
    }
    else if (hasTime(d)) {
        let n = d.toLocaleString([], {
            hour: "2-digit",
            minute: "2-digit",
        });
        var s = d.getDate() + '/' + (d.getMonth() + 1) + '/' + d.getFullYear();
        s += ' ' + n;
    } else {
        var s = (d.getDate() + '/' + d.getMonth() + 1) + '/' + d.getFullYear();
    }
    return s;
}
function hasTime(d) {
    return !!(d.getUTCHours() || d.getUTCMinutes() || d.getUTCSeconds());
}

function zeroFill(n) {
    if ((n + '').length == 1)
        return '0' + n;

    return n;
}
function parseMSDate(s) {
    if (s == undefined || s == null) {
        return "";
    }
    // Jump forward past the /Date(, parseInt handles the rest
    return new Date(parseInt(s.substr(6)));
}

function DisplayStatus(status) {
    if (status == 1) { //Not Appproved
        return '<lable class="form-control-label text-center text-primary">N</lable>'
    }
    else if (status == 2) { //Apporved
        return '<lable class="form-control-label text-center text-success">A</lable>'
    }
    else if (status == 3) { //Cancelled
        return '<lable class="form-control-label text-center text-danger">C</lable>'
    }
    else if (status == 4) { //Pending
        return '<lable class="form-control-label text-center text-primary">Pending</lable>'
    }
    else if (status == 5) { //Partial
        return '<lable class="form-control-label text-center" style="color:salmon;">Partial</lable>'
    }
    else if (status == 6) { //Forwarded
        return '<lable class="form-control-label text-center" style="color:deeppink;"><i class="fa fa-lock" aria-hidden="true"></i></lable>'
    }
    else if (status == 7) { //Partial Approved
        return '<lable class="form-control-label text-center" style="color:lightseagreen;">PA</lable>'
    }
    else { //Draft
        return '<lable class="form-control-label text-center text-info">Draft</lable>'
    }
}
function formatAmount(amount) {
    var currency = "INR";
    if (amount == null || amount == undefined || amount == "") {
        amount = 0;
    }
    var formatter = new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency: currency
    });
    return formatter.format(amount);
}