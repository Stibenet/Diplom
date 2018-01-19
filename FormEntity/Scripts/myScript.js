var hold = "";

var wfocus;
window.onfocus = function () {
    wfocus = true;
    blinkTitleStop();
};
window.onblur = function () {
    wfocus = false;
};


function blinkTitle(msg1, msg2, delay) {
    if (!wfocus) {
        var initialTitle = document.title;
        hold = window.setInterval(function () {
            if (document.title == msg1) {
                document.title = msg2;
            } else {

                document.title = msg1;
            }
        }, delay);
        new Audio("Content/msg.mp3").play();
    }
}

function blinkTitleStop() {
    clearInterval(hold);
    document.title = "Чат ЮГУ";
}