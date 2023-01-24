// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(document).ready(() => {
    $(`#table_${localStorage["page"]}`).toggleClass('d-none', false)
    $(`#button_${localStorage["page"]}`).toggleClass('btn-secondary', false)
    $(`#button_${localStorage["page"]}`).toggleClass('btn-primary', true)

    if (localStorage["workers_details"] == undefined) {
        localStorage["workers_details"] = JSON.stringify([])
    }
    var det = $.parseJSON(localStorage["workers_details"])
    for (let i = 0; i < det.length; i++) {
        $(`#${det[i]}`).attr('open', true)
    }

    $('#nav button').on('click', function () {
        CloseForms($(this).attr('id').replace('button_', ''))
        $('#nav button').removeClass('btn-primary')
        $('#nav button').toggleClass('btn-secondary', true)
        $(this).toggleClass('btn-secondary')
        $(this).toggleClass('btn-primary')
    })

    $('.workers_groups').on('toggle', function () {
        detailsOpen = []        
        console.log($(this).prop('open'))
        $('.workers_groups').each(function () {
            if ($(this).prop('open')) {
                detailsOpen.push($(this).prop('id'))
            }
        })
        localStorage["workers_details"] = JSON.stringify(detailsOpen)
    })
    
    // Изменение select в добавлении связи контроллер - группа доступа(sn)
    $('#add_relations_controllers_access_groups_sn_select').change(function () {
        let snSelect = document.getElementById('add_relations_controllers_access_groups_sn_select')
        let snInput = document.getElementById('add_relations_controllers_access_groups_sn')
        snInput.value = snSelect.value
    })

    // Изменение select в добавлении связи контроллер - группа доступа(accessGroup)
    $('#add_relations_controllers_access_groups_group_select').change(function () {
        let snSelect = document.getElementById('add_relations_controllers_access_groups_group_select')
        let snInput = document.getElementById('add_relations_controllers_access_groups_group')
        snInput.value = snSelect.value
    })
})

// Закрывает формы
function CloseForms(tableName) {
    $('#table_users').toggleClass('d-none', true)
    $('#table_workers').toggleClass('d-none', true)
    $('#table_cards').toggleClass('d-none', true)
    $('#table_controllers').toggleClass('d-none', true)
    $('#table_events').toggleClass('d-none', true)
    $('#table_groups').toggleClass('d-none', true)
    $('#table_access_groups').toggleClass('d-none', true)
    $('#table_relations_controllers_access_groups').toggleClass('d-none', true)
    $(`#table_${tableName}`).toggleClass('d-none', false)
    localStorage["page"] = tableName

    $('#add_users').toggleClass('d-none', true)
    $('#add_workers').toggleClass('d-none', true)
    $('#add_card_in_worker').toggleClass('d-none', true)
    $('#add_cards').toggleClass('d-none', true)
    $('#add_form_group').toggleClass('d-none', true)
    $('#add_form_access_group').toggleClass('d-none', true)
    $('#add_access_group_in_group').toggleClass('d-none', true)
    $('#add_relations_controllers_access_groups').toggleClass('d-none', true)
}

// Удаление пользователя
function DeleteUser(userId) {
    if (confirm(`Вы уверены что хотите удалить этого пользователя ID: ${userId}`)) {
        $.ajax({
            url: '/?handler=Delete',
            method: "GET",
            data: { id: userId, flag: "user" },
            success: () => {
                $('#user' + userId).toggleClass('d-none', true)
            }
        });
    }
}

// Удаление сотрудника
function DeleteWorker(workerId) {
    if (confirm(`Вы уверены что хотите удалить этого сотрудника ID: ${workerId}`)) {
        $.ajax({
            url: '/?handler=Delete',
            method: "GET",
            data: { id: workerId, flag: "worker" },
            success: function (success) {
                if (success == 'true') {
                    location.reload()
                }
            }
        });
    }
}

// Удаление карты
function DeleteCard(cardId) {
    if (confirm(`Вы уверены что хотите удалить эту карту ID: ${cardId}`)) {
        $.ajax({
            url: '/?handler=Delete',
            method: "GET",
            data: { id: cardId, flag: "card" },
            success: function (success) {
                if (success == 'true') {
                    location.reload()
                }
            }
        });
    }
}

// Удаление контроллера
function DeleteController(controllerId) {
    if (confirm(`Вы уверены что хотите удалить этот контроллер ID: ${controllerId}`)) {
        $.ajax({
            url: '/?handler=Delete',
            method: "GET",
            data: { id: controllerId, flag: "controller" },
            success: () => {     
                $('#controller' + controllerId).toggleClass('d-none', true)                                
            }
        });
    }
}

// Удаление группы
function DeleteGroup(groupId) {
    if (confirm(`Вы уверены что хотите удалить эту группу ID: ${groupId}`)) {
        $.ajax({
            url: '/?handler=Delete',
            method: "GET",
            data: { id: groupId, flag: "group" },
            success: function (success) {
                if (success == "true") {
                    location.reload()
                }      
                else {
                    alert(success)
                }
            }
        });
    }
}

// Удаление группа доступа
function DeleteAccessGroup(acessGroupId) {
    if (confirm(`Вы уверены что хотите удалить эту группу доступа ID: ${acessGroupId}`)) {
        $.ajax({
            url: '/?handler=Delete',
            method: "GET",
            data: { id: acessGroupId, flag: "acessGroup" },
            success: function (success) {
                if (success == 'true') {
                    location.reload()
                }
                else {
                    alert(success)
                }
            }
        });
    }
}

// Удаление связи контроллер - группа доступа
function DeleteRelationsControllersAccessGroups(relationId) {
    if (confirm(`Вы уверены что хотите удалить эту связь ID: ${relationId}`)) {
        $.ajax({
            url: '/?handler=Delete',
            method: "GET",
            data: { id: relationId, flag: "relation" },
            success: function (success) {
                if (success == 'true') {
                    location.reload()
                }
            }
        });
    }
}

// Показать форму добавления пользователя
function UserForm() {
    $('#add_users').toggleClass('d-none')
    $('#add_users_err').toggleClass('d-none', true)
}

// Показать форму добавления сотрудника
function WorkerForm() {
    $('#add_workers').toggleClass('d-none')
    $('#add_workers_err').toggleClass('d-none', true)
}

// Показать форму добавления карты
function CardForm() {
    $('#add_cards').toggleClass('d-none')
    $('#add_cards_err').toggleClass('d-none', true)
}

// Показать форму добавления групп
function GroupForm() {
    $('#add_form_group').toggleClass('d-none')
    $('#add_groups_err').toggleClass('d-none', true)
}

// Показать форму добавления групп доступа
function AccessGroupForm() {
    $('#add_form_access_group').toggleClass('d-none')
    $('#add_acces_group_err').toggleClass('d-none', true)
}

// Показать форму добавления связи контроллер - группа доступа
function RelationsControllersAccessGroupsForm() {
    $('#add_relations_controllers_access_groups').toggleClass('d-none')
    $('#add_relations_controllers_access_groups_err').toggleClass('d-none', true)
    $('#add_relations_controllers_access_groups_sn').prop('selectedIndex', -1)
    $('#add_relations_controllers_access_groups_group').prop('selectedIndex', -1)
}

// Показать форму добавления карты сотруднику
function AddCardInWorkerForm(workerId) {
    $('#add_card_in_worker').toggleClass('d-none', false)
    $('#add_card_in_worker_err').toggleClass('d-none', true)
    let cardId = document.getElementById('add_card_in_worker_idn')
    cardId.value = workerId
}

// Показывает форму добавления группы доступа в группу
function AddAccessGroupInGroupForm(groupId) {
    $.ajax({
        url: '/?handler=AccessGroupInAddGroupForm',
        method: "GET",
        data: { groupId: groupId},
        success: function (data) {            
            $('#add_access_group_in_group').toggleClass('d-none', false)
            $('#add_access_group_in_group_err').toggleClass('d-none', true)
            let group = document.getElementById('add_access_group_in_group_name')
            group.value = groupId

            $('#add_access_group_in_group_select').prop('selectedIndex', -1)
            var result = $.parseJSON(data)
            $('.add_access_group_in_group_option').toggleClass('d-none', false)
            for (let i = 0; i < result.length; i++) {
                $('#add_access_group_in_group_option_' + result[i]).toggleClass('d-none', true)
            }
        }
    });
}

// Добавление пользователя
function AddUser() {
    let insertLogin = document.getElementById('add_user_log')
    let insertPassword = document.getElementById('add_user_passw')
    let log = insertLogin.value
    let pas = insertPassword.value
    $.ajax({
        url: '/?handler=AddUser',
        method: "GET",
        data: { login: log, password: pas },
        success: function (success) {
            if (success == 'true') {
                $('#add_users').toggleClass('d-none', true)
                location.reload()
            }
            else {
                $('#add_users_err').toggleClass('d-none', false)
                let err = document.getElementById('add_users_err')
                err.innerHTML = success
            }
        }
    });
}

// Добавление сотрудника
function AddWorker() {
    let insertWorkerIdn = document.getElementById('add_worker_idn')
    let insertWorkerFio = document.getElementById('add_worker_fio')
    let insertWorkerPosition = document.getElementById('add_worker_position')
    let insertWorkerComment = document.getElementById('add_worker_comment')
    let insertWorkerCard = document.getElementById('add_worker_card')
    let selectGroup = document.getElementById('add_worker_select_group')
    let workerIdn = insertWorkerIdn.value
    let workerFio = insertWorkerFio.value
    let workerPosition = insertWorkerPosition.value
    let workerComment = insertWorkerComment.value
    let workerCard = insertWorkerCard.value
    let workerGroup = selectGroup.value
    $.ajax({
        url: '/?handler=AddWorker',
        method: "GET",
        data: { worker: workerIdn, fio: workerFio, card: workerCard, position: workerPosition, comment: workerComment, group: workerGroup },
        success: function (success) {
            if (success == 'true') {
                location.reload()
            }
            else {
                $('#add_workers_err').toggleClass('d-none', false)
                let err = document.getElementById('add_workers_err')
                err.innerHTML = success
            }
        }
    });
}

// Выбор карты при добавление сотрудника
function AddWorkerСhooseCard() {
    let selectElement = document.getElementById('add_worker_select_card')
    let cardId = document.getElementById('add_worker_card')
    cardId.value = selectElement.value
}

// Считывание карты при добавлении сотрудника
function AddWorkerReadCard() {
    $('button').attr('disabled', true)
    $.ajax({
        url: '/?handler=ReadCard',
        method: "GET",
        data: {},
        success: function (success) {
            $('button').attr('disabled', false)
            if (success == 'false') {
                $('#add_workers_err').toggleClass('d-none', false)
                let err = document.getElementById('add_workers_err')
                err.innerHTML = 'не удалось считать карту'
            }
            else {
                let cardId = document.getElementById('add_worker_card')
                cardId.value = success
            }
        }
    });
}

// Добавления карты сотруднику
function AddCardInWorker() {
    let insertCard = document.getElementById('add_card_in_worker_card')
    let insertWorker = document.getElementById('add_card_in_worker_idn')
    let card = insertCard.value
    let worker = insertWorker.value
    $.ajax({
        url: '/?handler=AddCardInWorker',
        method: "GET",
        data: { card: card, worker: worker },
        success: function (success) {
            if (success == 'newcard') {
                alert('Этой карты не было в базе данных, поэтому была создана новая карта')
                location.reload()
            }
            if (success == 'true') {
                location.reload()
            }
            else {
                $('#add_card_in_worker_err').toggleClass('d-none', false)
                let err = document.getElementById('add_card_in_worker_err')
                err.innerHTML = success
            }
        }
    });
}

// Убирание карты сотрудника
function RemoveCardFromWorker(worker, Id) {
    if (confirm("Вы уверены что хотите убрать эту карту")) {
        let selectElement = document.getElementById(`worker${Id}_select_card`)
        let card = selectElement.value
        $.ajax({
            url: '/?handler=RemoveCardFromWorker',
            method: "GET",
            data: { cardId: card, worker: worker },
            success: function (success) {
                if (success == 'true') {
                    location.reload()
                }
                else {
                    $('#add_card_in_worker_err').toggleClass('d-none', false)
                    let err = document.getElementById('add_card_in_worker_err')
                    err.innerHTML = success
                }
            }
        });
    }
}

// Добавление карты сотруднику
function AddCardInWorkerСhooseCard() {
    let selectElement = document.getElementById('add_card_in_worker_select_card')
    let cardId = document.getElementById('add_card_in_worker_card')
    cardId.value = selectElement.value
}

// Считывание карты при добавлении карты сотруднику
function AddCardInWorkerReadCard() {
    $('button').attr('disabled', true)
    $.ajax({
        url: '/?handler=ReadCard',
        method: "GET",
        data: {},
        success: function (success) {
            $('button').attr('disabled', false)
            if (success == 'false') {
                $('#add_card_in_worker_err').toggleClass('d-none', false)
                let err = document.getElementById('add_card_in_worker_err')
                err.innerHTML = 'не удалось считать карту'
            }
            else {
                let cardId = document.getElementById('add_card_in_worker_card')
                cardId.value = success
            }
        }
    });
}

// Изменение индивидуальных групп доступа
function ChangeAccessGroups(workerId) {
    let checkboxJson = [];
    $(`.Modal_worker_access_groups${workerId}_checkbox:checkbox:checked`).each(function () {
        checkboxJson.push($(this).val());
    })
    $.ajax({
        url: '/?handler=ChangeAccessGroups',
        method: "GET",
        data: { workerId: workerId, checkboxJson: JSON.stringify(checkboxJson) },
        success: function (data) {
            if (data == 'true') {
                location.reload()
            }
        }
    });
}

// Изменение индивидуальных групп доступа
function ChangePersonalCheck(workerId) {
    $.ajax({
        url: '/?handler=ChangePersonalCheck',
        method: "GET",
        data: { workerId: workerId }
    });
}

// Редактирование сотрудника
function RedactWorker(workerId) {
    let workerNew = $(`#Modal_worker_redact${workerId}_worker`).val()
    let fio = $(`#Modal_worker_redact${workerId}_fio`).val()
    let position = $(`#Modal_worker_redact${workerId}_position`).val()
    let group = $(`#Modal_worker_redact${workerId}_select_group option:selected`).text()
    let comment = $(`#Modal_worker_redact${workerId}_comment`).val()
    $.ajax({
        url: '/?handler=RedactWorker',
        method: "GET",
        data: { id: workerId, workerNew: workerNew, fio: fio, position: position, group: group, comment: comment },
        success: function (success) {
            if (success == 'true') {
                location.reload()
            }
            else {
                let err = $(`#Modal_worker_redact${workerId}_err`)
                err.toggleClass('d-none', false)
                err.html(success)
            }
        }
    });
}

// Добавление карты
function AddCard() {
    let insertCard = document.getElementById('add_card_id')
    let insertWorker = document.getElementById('add_card_worker')
    let card = insertCard.value
    let worker = insertWorker.value
    $.ajax({
        url: '/?handler=AddCard',
        method: "GET",
        data: { card: card, worker: worker},
        success: function (success) {
            if (success == 'true') {
                location.reload()
            }
            else {
                $('#add_cards_err').toggleClass('d-none', false)
                let err = document.getElementById('add_cards_err')
                err.innerHTML = success
            }
        }
    });
}

// Считывание карты при добавлении карты
function ReadCard() {
    $('button').attr('disabled', true)
    $.ajax({
        url: '/?handler=ReadCard',
        method: "GET",
        data: {},
        success: function (success) {
            $('button').attr('disabled', false)
            if (success == 'false') {
                $('#add_cards_err').toggleClass('d-none', false)
                let err = document.getElementById('add_cards_err')
                err.innerHTML = 'не удалось считать карту'
            }
            else {
                let cardId = document.getElementById('add_card_id')
                cardId.value = success
            }
        }
    });
}

// Добавление группы
function AddGroup() {
    let insertGroup = document.getElementById('add_group_name')
    let group = insertGroup.value
    $.ajax({
        url: '/?handler=AddGroup',
        method: "GET",
        data: { group: group },
        success: function (success) {
            if (success == 'true') {
                location.reload()
            }
            else {
                $('#add_groups_err').toggleClass('d-none', false)
                let err = document.getElementById('add_groups_err')
                err.innerHTML = success
            }
        }
    });
}

// Добавление группы доступа в группу
function AddAccessGroupInGroup() {
    let accessGroup = $('#add_access_group_in_group_select option:selected').text()
    let groupId = $('#add_access_group_in_group_name').val()
    $.ajax({
        url: '/?handler=AddAccessGroupInGroup',
        method: "GET",
        data: { groupId: groupId, accessGroup: accessGroup },
        success: function (success) {
            if (success == 'true') {
                location.reload()
            }
            else {
                $('#add_access_group_in_group_err').toggleClass('d-none', false)
                let err = document.getElementById('add_access_group_in_group_err')
                err.innerHTML = success
            }
        }
    });
}

// Убирание группы доступа из группы
function RemoveAccessGroupFromGroup(groupId) {
    if (confirm("Вы уверены что хотите убрать эту группу доступа")) {
        let selectElement = document.getElementById(`group${groupId}_select_access_group`)
        let accessGroup = selectElement.value
       
        $.ajax({
            url: '/?handler=RemoveAccessGroupFromGroup',
            method: "GET",
            data: { groupId: groupId, accessGroup: accessGroup },
            success: function (success) {
                if (success == 'true') {
                    location.reload()
                }
                else {
                    alert(success)
                }
            }
        });
    }
}

// Добавление группы доступа
function AddAccessGroup() {
    let insertGroup = document.getElementById('add_access_group_name')
    let accessGroup = insertGroup.value
    $.ajax({
        url: '/?handler=AddAccessGroup',
        method: "GET",
        data: { accessGroup: accessGroup },
        success: function (success) {
            if (success == 'true') {
                location.reload()
            }
            else {
                $('#add_acces_group_err').toggleClass('d-none', false)
                let err = document.getElementById('add_acces_group_err')
                err.innerHTML = success
            }
        }
    });
}

// Добавление связи контроллер - группа доступа
function AddRelationsControllersAccessGroups() {
    let sn = $(`#add_relations_controllers_access_groups_sn option:selected`).text()
    let accessGroup = $(`#add_relations_controllers_access_groups_group option:selected`).text()
    let cbIn = $(`#add_relations_controllers_access_groups_in`).is(':checked')
    let cbOut = $(`#add_relations_controllers_access_groups_out`).is(':checked')
    $.ajax({
        url: '/?handler=AddRelationsControllersAccessGroups',
        method: "GET",
        data: { sn: sn, accessGroup: accessGroup, cbIn: cbIn, cbOut: cbOut},
        success: function (success) {
            if (success == 'true') {
                location.reload()
            }
            else {
                $('#add_relations_controllers_access_groups_err').toggleClass('d-none', false)
                let err = document.getElementById('add_relations_controllers_access_groups_err')
                err.innerHTML = success;
                console.log(success);
            }
        }
    });
}

// Поиск по картам
function FilterCard() {
    let filter = document.getElementById('filter_card').value
    $.ajax({
        url: '/?handler=FilterCard',
        method: "GET",
        data: { filter: filter },
        success: function (data) {
            if (data == 'false') {
                $('.card_tr').toggleClass('d-none', false)            
            }
            else 
            {
                var result = $.parseJSON(data)
                $('.card_tr').toggleClass('d-none', true)
                for (let i = 0; i < result.length; i++) {
                    $('#card' + result[i]).toggleClass('d-none', false)
                }
            }
        }
    });
}

// Поиск по сотрудникам
function FilterWorker() {
    let filter = document.getElementById('filter_worker').value
    $.ajax({
        url: '/?handler=FilterWorker',
        method: "GET",
        data: { filter: filter },
        success: function (data) {
            if (data == 'false') {
                $('.worker_tr').toggleClass('d-none', false) 
                $('.workers_groups').toggleClass('d-none', false)
            }
            else {                
                var result = $.parseJSON(data)
                $('.worker_tr').toggleClass('d-none', true)
                $('.workers_groups').toggleClass('d-none', true)
                for (let i = 0; i < result.length; i++) {
                    const words = result[i].split(';')
                    $(`#worker${words[0]}`).toggleClass('d-none', false)
                    $(`#workers_group_${words[1]}`).toggleClass('d-none', false)
                }
            }
        }
    });
}

// Поиск по связям контроллер - группа доступа
function FilterRelationsControllersAccessGroups() {
    let filter = document.getElementById('filter_relations_controllers_access_groups').value
    $.ajax({
        url: '/?handler=FilterRelationsControllersAccessGroups',
        method: "GET",
        data: { filter: filter },
        success: function (data) {
            if (data == 'false') {
                $('.relations_controllers_access_groups_tr').toggleClass('d-none', false)
            }
            else {
                var result = $.parseJSON(data)
                $('.relations_controllers_access_groups_tr').toggleClass('d-none', true)
                for (let i = 0; i < result.length; i++) {
                    $('#relation' + result[i] + '_controllers_access_groups_tr').toggleClass('d-none', false)
                }
            }
        }
    });
}