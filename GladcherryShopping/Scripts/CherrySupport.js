function PlaySound() {
    var audio = document.getElementById("myAudio");
    audio.play();
}


function getCookie(cname) {
    var name = cname + "=";
    var decodedCookie = decodeURIComponent(document.cookie);
    var ca = decodedCookie.split(';');
    for(var i = 0; i <ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) == 0) {
            return c.substring(name.length, c.length);
        }
    }
    return "";
}

$(function () {
    // Declare a proxy to reference the hub.
    var chat = $.connection.chatHub;
    // Passing Query String Data
    var data = getCookie("chruid");
    if (data != null && data != "") {
        $.connection.hub.qs = { "Type" : 3, "OperatorId": data };
    } else {
        $.connection.hub.qs = { "Type" : 3, "UserGeneratedId": "" };
    }
    // Create a function that the hub can call to broadcast messages.
    chat.client.attachOperatorMessage = function (image, name, message, date) {
        // Html encode display name and message.
        var encodedName = $('<p />').text(name).html();
        var encodedMsg = $('<p />').text(message).html();
        var encodedInput = $('<input value="' + message + '" />');
        // Add the message to the page.
        PlaySound();
        createLeftRow('<li><div class="left-chat"><img src="' + image + '"><p>' + message + '</p><span>' + date + '</span></div></li>');
        //$('div.chat-section > ul').append('<li><div class="left-chat"><img src="' + image + '"><p>' + message + '</p><span>' + date + '</span></div></li>');
    };
    //$('#message').focus();
    // Start the connection.
    $.connection.hub.start().done(function () {
        $('.a-send-message').click(function () {
            data = getCookie("chruid");
            // Call the Send method on the hub.
            chat.server.sendGuestChatMessage($('input.user-entry').val(), data);
            createRow($('.user-entry'));
            // Clear text box and reset focus for next comment.
            $('input.user-entry').val('').focus();
        });
        $('.user-entry').on("keydown", function(e){
            // if user pressed enter
            if (e.which == 13) {
                data = getCookie("chruid");
                chat.server.sendGuestChatMessage($('input.user-entry').val(), data);
                createRow($('.user-entry'));
                // Clear text box and reset focus for next comment.
                $('input.user-entry').val('').focus();
            }
        });
    });
    $.connection.hub.disconnected(function() {
        setTimeout(function() {
            $.connection.hub.start().done(function () {
                $('.a-send-message').click(function () {
                    // Call the Send method on the hub.
                    chat.server.sendGuestChatMessage($('input.user-entry').val(), data);
                    createRow($('.user-entry'));
                    // Clear text box and reset focus for next comment.
                    $('input.user-entry').val('').focus();
                });
            });
        }, 5000); // Restart connection after 5 seconds.
    });
});