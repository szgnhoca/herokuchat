const connection = new signalR.HubConnectionBuilder()
    .withUrl("/message-hub",
        {
            withCredentials: true,
            headers: { Cookie: getCookieValue("auth-cookie") }
        }).build();

$(function () {
    connection.start().catch(err => console.error(err.toString())).then(function () {
        connection.send("Connect");
        connection.on('onConnected', (persons) => onConnected(persons));
        connection.on('onNewUserConnected', (person) => onNewUserConnected(person));
        connection.on('onUserDisconnected', (person) => onUserDisconnected(person));
        connection.on('receivePrivateMessage', (fromUser, message) => receivePrivateMessage(fromUser, message));
    });

    $('body').on('click', 'a.list-group-item[data-user-id]', function (e) {
        const $user = $(e.currentTarget);
        const = $('.counter', $user).text('');
        const $userId = $user.data('user-id');
        const $template = $('#chat-item-template').html()
            .replace("{userId}", $userId)
            .replace("{name}", $user.text());
        $('#chat-area').html($template);
        $.ajax('home/history?otherMember=' + $userId)
            .done(function (resp) {
                const $chatArea = $('#chat-area');
                if ($chatArea.length > 0) {
                    for (var i = 0; i < resp.length; i++) {
                        const msg = resp[i].message;
                        const leftOrRight = resp[i].mine ? "right" : "left";
                        $('.history ul', $chatArea)
                            .prepend('<li class="m-1"><p class="text-' + leftOrRight + '">' + msg + '</p></li>')
                    }
                }
            });
    });

    $('body').on('keypress', '#message-box',
        function (e) {
            if (e.which === 13) {
                var text = e.currentTarget.value;
                if (text.replace(" ", "") == "") return;
                const userId = $(e.currentTarget).parent().data('user-id');
                connection.send("SendPrivateMessage", userId, text);
                $(e.currentTarget).val('');
                const $chatArea = $('#chat-area');
                if ($chatArea.length > 0) {
                    $('.history ul', $chatArea)
                        .append('<li class="m-1"><p class="text-right">' + text + '</p></li>')
                }
            }
        });
});

function onConnected(persons) {
    for (var i = 0; i < persons.length; i++) {
        bindPersonToList(persons[i]);
    }
}

function onNewUserConnected(person) {
    bindPersonToList(person);
}

function onUserDisconnected(person) {
    removePersonFromList(person);
}

function onUserDisconnected(person) {
    removePersonFromList(person);
}

function receivePrivateMessage(fromUser, message) {
    const $chatArea = $('[data-user-id=' + fromUser.id + ']', '#chat-area');
    const $target = $('#peoples [data-user-id=' + fromUser.id + ']');
    const $fromUserCounter = $('.counter', $target);

    if ($chatArea.length > 0) {
        $('.history ul', $chatArea)
            .append('<li class="m-1"><p class="text-left">' + message + '</p></li>')
    }
    else {
        if ($fromUserCounter.text() === "") {
            $fromUserCounter.text("1")
        } else {
            $fromUserCounter.text(parseInt($fromUserCounter.text()) + 1)
        }
    }
}

//Utils START
function removePersonFromList(person) {
    const $target = $('#peoples');
    const $personToDelete = $('[data-user-id=' + person.id + ']', $target);
    $personToDelete.remove();
}

function bindPersonToList(person) {
    const $target = $('#peoples');
    var contains = $('[data-user-id=' + person.id + ']', $target).length !== 0;
    if (!contains) {
        const template = $('#user-item-template').html()
            .replace("{name}", person.name)
            .replace("{userId}", person.id);
        $target.append(template);
    }
}

function getCookieValue(name) {
    var b = document.cookie.match('(^|;)\\s*' + name + '\\s*=\\s*([^;]+)');
    return b ? b.pop() : '';
}
//Utils END