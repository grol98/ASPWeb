// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(document).ready(() => {
    $(`#table_${localStorage["page"]}`).toggleClass('d-none', false)
    $(`#button_${localStorage["page"]}`).toggleClass('btn-secondary', false)
    $(`#button_${localStorage["page"]}`).toggleClass('btn-primary', true)   

    if (localStorage["page"] == "workers") {
        if (localStorage["workers_details"] == undefined) {
            localStorage["workers_details"] = JSON.stringify([])
        }
        var det = $.parseJSON(localStorage["workers_details"])
        for (let i = 0; i < det.length; i++) {
            $(`#${det[i]}`).attr('open', true)
        }
    }

    //if (localStorage["page"] == "events") {
    //    $("#filter_events_select_group").prop("selectedIndex", -1);
    //}

    $('#nav button').on('click', function () {
        ChangeTab($(this).attr('id').replace('button_', ''))
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


//function Scroll() {
//    $('html, body').animate({
//        scrollTop: $("#worker4542").offset().top
//    }, 100);
//}

function LogOn() {    
    let login = $("#user_login").attr('value')
    let password = $("#user_password").attr('value')
    $.ajax({
        url: '/?handler=LogOn',
        method: "GET",
        data: { login: login, password: password, tab: localStorage["page"] },
        success: function (success) {
            if (success == 'true') {
                location.reload()
            }
            else {
                $("#user_login").val() = success
            }
        }
    });
}

// Закрывает формы
function ChangeTab(tableName) {
    localStorage["page"] = tableName
    $.ajax({
        url: '/?handler=ChangePage',
        method: "GET",
        data: { tab: tableName },
        success: function (success) {
            if (success == 'true') {
                location.reload()
            }
        }
    });
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

// Удаление сотрудника
function DeleteImage(workerId) {
    if (confirm(`Вы уверены что хотите удалить эту фотографию`)) {
        $.ajax({
            url: '/?handler=DeleteImage',
            method: "GET",
            data: { workerId: workerId },
            success: function (success) {
                if (success == 'true') {
                    location.reload()
                }
                else {
                    let err = $(`#Modal_worker_edit_err`)
                    err.toggleClass('d-none', false)
                    err.html(success)
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
    //$('#add_relations_controllers_access_groups_sn').prop('selectedIndex', -1)
   // $('#add_relations_controllers_access_groups_group').prop('selectedIndex', -1)
}

// Показать форму добавления карты сотруднику
function AddCardInWorkerForm(workerId) {
    $('#add_card_in_worker').toggleClass('d-none', false)
    $('#add_card_in_worker_err').toggleClass('d-none', true)
    let cardId = document.getElementById('add_card_in_worker_id')
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


// Показывает форму изменения групп доступа
function ShowModalAccessGroups(workerId) {
    $('#ModalLabel_worker_access_groups').html(`Индивидуальные группы доступа для ID сотрудника: ${workerId}`);
    $('#ModalLabel_worker_access_groups').attr(`worker-id`, `${workerId}`);
    $.ajax({
        url: '/?handler=ShowModalAccessGroups',
        method: "GET",
        data: { workerId: workerId },
        success: function (data) {
            var result = $.parseJSON(data);
            $('.Modal_worker_access_groups_checkbox').prop('checked', false);
            for (let i = 0; i < result.length; i++) {
                $(`#${result[i]}`).prop('checked', true);
            }
        }
    });
}


// Показывает форму изменения точек доступа
function ShowModalAccessPoints(workerId) {
    $('#ModalLabel_worker_access_points').html(`Индивидуальные точки доступа для ID сотрудника: ${workerId}`);
    $('#ModalLabel_worker_access_points').attr(`worker-id`, `${workerId}`);
    $.ajax({
        url: '/?handler=ShowModalAccessPoints',
        method: "GET",
        data: { workerId: workerId },
        success: function (data) {
            var result = $.parseJSON(data);
            $('.Modal_worker_access_points_checkbox').prop('checked', false);
            for (let i = 0; i < result.length; i++) {
                $(`#${result[i]}`).prop('checked', true);
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
    var formData = new FormData(Image_edit)

    $.ajax({
        type: 'POST',
        url: '/?handler=SetImage',
        data: formData,
        cache: false,
        contentType: false,
        processData: false,
        success: function (data) {
            if (data == 'false') {
                let err = $(`#Modal_worker_edit_err`)
                err.toggleClass('d-none', false)
                err.html(data)
            }
            else {
                let worker = $(`#Modal_worker_edit_worker`).val()
                let LastName = $(`#Modal_worker_edit_lastname`).val()
                let FirstName = $(`#Modal_worker_edit_firstname`).val()
                let FatherName = $(`#Modal_worker_edit_fathername`).val()
                let position = $(`#Modal_worker_edit_position`).val()
                let group = $(`#Modal_worker_edit_select_group option:selected`).text()
                let comment = $(`#Modal_worker_edit_comment`).val()
                let card = $('#add_worker_card').val()
                $.ajax({
                    url: '/?handler=AddWorker',
                    method: "GET",
                    data: { worker: worker, LastName: LastName, FirstName: FirstName, FatherName: FatherName, card: card, position: position, group: group, comment: comment, image: data },
                    success: function (success) {
                        if (success == 'true') {
                            location.reload()
                        }
                        else {
                            console.log(success)
                            let err = $(`#Modal_worker_edit_err`)
                            err.toggleClass('d-none', false)
                            err.html(success)
                        }

                    }
                });
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
async function AddWorkerReadCard() {
    if (!this.serialPortHandler.isOpened) {
        await this.serialPortHandler.open();
    }
    const message = await this.serialPortHandler.read()
    let cardId = document.getElementById('add_worker_card')
    cardId.value = message;
}
        }
    });
}

// Добавления карты сотруднику
function AddCardInWorker() {
    let card = $(`#add_card_in_worker_card`).val();
    let workerId = $(`#add_card_in_worker_id`).val();
    if ($(`#worker${workerId}_select_card option`).length > 0 && !confirm("Уверены, что хотите создать пропуск? У сотрудника он уже есть.")) {
        $(`#Modal_add_card_in_worker`).modal('toggle');
        return;
    }
    
    $.ajax({
        url: '/?handler=AddCardInWorker',
        method: "GET",
        data: { card: card, workerId: workerId },
        success: function (success) {
            if (success == 'newcard') {
                alert('Этой карты не было в базе данных, поэтому была создана новая карта');
                $(`#worker${workerId}_select_card`).append(`<option value="${card}">${card}</option>`);
                $(`#Modal_add_card_in_worker`).modal('toggle');
                if ($(`#worker${workerId}_select_card option`).length == 1) {
                    $(`#remove_card${workerId}`).toggleClass('d-none', false);
                }
            if (success == 'true') {
                location.reload()
            }
            else if (success == 'true') {
                $(`#worker${workerId}_select_card`).append(`<option value="${card}">${card}</option>`);
                $(`#Modal_add_card_in_worker`).modal('toggle');
                $(`#add_card_in_worker_select_card option[value="${card}"]`).remove();
                if ($(`#worker${workerId}_select_card option`).length == 1) {
                    $(`#remove_card${workerId}`).toggleClass('d-none', false);
                }
            }
            else {
                $('#add_card_in_worker_err').toggleClass('d-none', false)
                let err = document.getElementById('add_card_in_worker_err');
                err.innerHTML = success;
            }
        }
    });
}

// Убирание карты сотрудника
function RemoveCardFromWorker(workerId) {
    if (confirm("Вы уверены что хотите убрать эту карту")) {        
        let card = $(`#worker${workerId}_select_card`).val();
        $.ajax({
            url: '/?handler=RemoveCardFromWorker',
            method: "GET",
            data: { card: card, workerId: workerId },
            success: function (success) {
                if (success == 'true') {
                    $(`#worker${workerId}_select_card option[value='${card}']`).remove();
                    $(`#add_card_in_worker_select_card`).append(`<option value="${card}">${card}</option>`);
                    if ($(`#worker${workerId}_select_card option`).length == 0) {
                        $(`#remove_card${workerId}`).toggleClass('d-none', true);
                    }
                }
                else {
                    alert(success);
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


this.serialPortHandler = new SerialPortHandler(
    { baudRate: 9600 });
// Считывание карты при добавлении карты сотруднику
async function AddCardInWorkerReadCard() {
    if (!this.serialPortHandler.isOpened) {
        await this.serialPortHandler.open();
    }
    const message = await this.serialPortHandler.read()
    let cardId = document.getElementById('add_card_in_worker_card')
    cardId.value = message;
}


// Удаление всех карт из контроллеров
function DeleteAllCardFromController() {
    let checkboxJson = [];
    $(`.Modal_delete_cards_from_controllers_checkbox:checkbox:checked`).each(function () {
        checkboxJson.push($(this).val());
    })
    $.ajax({
        url: '/?handler=DeleteAllCardFromController',
        method: "GET",
        data: { checkboxJson: JSON.stringify(checkboxJson) }
    });
    $('#Modal_delete_cards_from_controllers').modal('toggle');
}

// Загрузка всех карт в контроллеров
function AddAllCardInController() {
    let checkboxJson = [];
    $(`.Modal_add_cards_in_controllers_checkbox:checkbox:checked`).each(function () {
        checkboxJson.push($(this).val());
    })
    $.ajax({
        url: '/?handler=AddAllCardInController',
        method: "GET",
        data: { checkboxJson: JSON.stringify(checkboxJson) }
    });
    $('#Modal_add_cards_in_controllers').modal('toggle');
}


// Изменение индивидуальных групп доступа
function ChangeAccessGroups() {
    let workerId = $('#ModalLabel_worker_access_groups').attr('worker-id');
    let checkboxJson = [];
    $(`.Modal_worker_access_groups_checkbox:checkbox:checked`).each(function () {
        checkboxJson.push($(this).val());
    })
    $.ajax({
        url: '/?handler=ChangeAccessGroups',
        method: "GET",
        data: { workerId: workerId, checkboxJson: JSON.stringify(checkboxJson) },
        success: function (data) {
            if (data == 'true') {
                $('#Modal_worker_access_groups').modal('toggle');
            }
        }
    });
}

// Изменение индивидуальных точек доступа
function ChangeAccessPoints() {
    let workerId = $('#ModalLabel_worker_access_points').attr('worker-id')
    let checkboxJson = [];
    $(`.Modal_worker_access_points_checkbox:checkbox:checked`).each(function () {
        checkboxJson.push($(this).val());
    })
    $.ajax({
        url: '/?handler=ChangeAccessPoints',
        method: "GET",
        data: { workerId: workerId, checkboxJson: JSON.stringify(checkboxJson) },
        success: function (data) {
            if (data == 'true') {
                $('#Modal_worker_access_points').modal('toggle');
            }
        }
    });
}


// Редактирование сотрудника
function EditWorker(id) {
    var formData = new FormData(Image_edit)

    $.ajax({
        type: 'POST',
        url: '/?handler=SetImage',
        data: formData,
        cache: false,
        contentType: false,
        processData: false,
        success: function (data) {
            if (data == 'false') {
                let err = $(`#Modal_worker_edit_err`)
                err.toggleClass('d-none', false)
                err.html(data)
            }
            else {
                let workerNew = $(`#Modal_worker_edit_worker`).val()
                let LastName = $(`#Modal_worker_edit_lastname`).val()
                let FirstName = $(`#Modal_worker_edit_firstname`).val()
                let FatherName = $(`#Modal_worker_edit_fathername`).val()
                let position = $(`#Modal_worker_edit_position`).val()
                let group = $(`#Modal_worker_edit_select_group option:selected`).text()
                let comment = $(`#Modal_worker_edit_comment`).val()
                $.ajax({
                    url: '/?handler=EditWorker',
                    method: "GET",
                    data: { id: id, workerNew: workerNew, LastName: LastName, FirstName: FirstName, FatherName: FatherName, position: position, group: group, comment: comment, image: data },
                    success: function (success) {
                        if (success.startsWith("Error")) {
                            console.log(success);
                            let err = $(`#Modal_worker_edit_err`);
                            err.toggleClass('d-none', false);
                            err.html(success);
                        }
                        else {
                            $(`#worker${id}`).html(success);
                            $('#Modal_worker_edit').modal('toggle');
                        }
                        
                    }
                });
            }
        }
    });
}


// Добавление даты блокировки
function DeleteLockDate(id) {
    $(`#lock_date`).val("");
    $.ajax({
        url: '/?handler=DeleteLockDate',
        method: "GET",
        data: { id: id }
    });
    $('#add_lock_date_button').prop('disabled', false);
    $('#delete_lock_date_button').prop('disabled', true);
    
}

// Добавление даты блокировки
function AddLockDate(id) {
    var lockDate = $(`#lock_date`).val();
    $.ajax({
        url: '/?handler=AddLockDate',
        method: "GET",
        data: { id: id, lockDate: lockDate },
         success: function (success) {
            if (success == 'true') {
                $('#Modal_worker_edit_err').toggleClass('d-none', true);
                $('#add_lock_date_button').prop('disabled', true);
                $('#delete_lock_date_button').prop('disabled', false);
            }
            else {
                $('#Modal_worker_edit_err').toggleClass('d-none', false);
                $('#Modal_worker_edit_err').html(success);
            }
        }
    });
}


// Добавление карты
function AddCard() {
    let insertCard = document.getElementById('add_card_id')
    let insertWorker = document.getElementById('add_card_worker')
    let card = insertCard.value
    let workerId = insertWorker.value
    $.ajax({
        url: '/?handler=AddCard',
        method: "GET",
        data: { card: card, workerId: workerId },
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
async function ReadCard() {
    if (!this.serialPortHandler.isOpened) {
        await this.serialPortHandler.open();
    }
    const message = await this.serialPortHandler.read()
    let cardId = document.getElementById('add_card_id')
    cardId.value = message;
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
    let sn = $(`#add_relations_controllers_access_groups_sn option:selected`).val()
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
    $('.table-card-spinner').removeClass('d-none');
    $.ajax({
        url: '/?handler=FilterCard',
        method: "GET",
        data: { filter: filter },
        success: function (data) {
            $('.table-card-spinner').addClass('d-none');
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
        },
        error: (error) => {
            $('.table-card-spinner').addClass('d-none');
            console.log(error.responseText)
        }
    });
}


// Поиск по сотрудникам
function FilterWorker() {
    let filter = document.getElementById('filter_worker').value
    $('.table-card-spinner').removeClass('d-none');
    $.ajax({
        url: '/?handler=FilterWorker',
        method: "GET",
        data: { filter: filter },
        success: function (data) {
            $('.table-card-spinner').addClass('d-none');
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


// Поиск сотрудников с комментариями
function FindComments() {
    $.ajax({
        url: '/?handler=FindComments',
        method: "GET",
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


// Поиск по событиям
function FilterEvents() {
    let checkboxJson = [];
    $(`.Modal_filter_sn_checkbox:checkbox:checked`).each(function () {
        checkboxJson.push($(this).val());
    })
    var workerId = $('#filter_events_worker').val();
    var fio = $('#filter_events_fio').val();
    var dateBeg = $('#filter_events_date_beg').val();
    var dateEnd = $('#filter_events_date_end').val();
    let group = $('#filter_events_select_group').val();
    $('.table-event-spinner').removeClass('d-none');
    $.ajax({
        url: '/?handler=FilterEvents',
        method: "GET",
        data: { snCheckboxJson: JSON.stringify(checkboxJson), workerId: workerId, fio: fio, group: group, dateBeg: dateBeg, dateEnd: dateEnd },
        success: function (html) {
            $('.table-event-spinner').addClass('d-none');
            $('#table_events tbody').html(html);
        },
        error: () => {
            $('.table-event-spinner').addClass('d-none');
            $('#table_events tbody').html('<p>Ошибка получения данных</p>');
        }
    });
}


// Отчет по событиям
function ReportEvents() {
    let checkboxJson = [];
    $(`.Modal_filter_sn_checkbox:checkbox:checked`).each(function () {
        checkboxJson.push($(this).val());
    })
    var workerId = $('#filter_events_worker').val();
    var fio = $('#filter_events_fio').val();
    var dateBeg = $('#filter_events_date_beg').val();
    var dateEnd = $('#filter_events_date_end').val();
    let group = $('#filter_events_select_group').val();
    $('.table-event-spinner').removeClass('d-none');
    $.ajax({
        url: '/?handler=ReportEvents',
        method: "GET",
        data: { snCheckboxJson: JSON.stringify(checkboxJson), workerId: workerId, fio: fio, group: group, dateBeg: dateBeg, dateEnd: dateEnd },
        success: function (data) {
            $('.table-event-spinner').addClass('d-none');
            console.log(data)
            var link = document.createElement('a');
            link.setAttribute('href', data);
            link.setAttribute('download', 'download.xlsx');
            link.click();
        }
    });
}


// Печать отчета по событиям
function PrintEvents() {
    let checkboxJson = [];
    $(`.Modal_filter_sn_checkbox:checkbox:checked`).each(function () {
        checkboxJson.push($(this).val());
    })
    var workerId = $('#filter_events_worker').val();
    var fio = $('#filter_events_fio').val();
    var dateBeg = $('#filter_events_date_beg').val();
    var dateEnd = $('#filter_events_date_end').val();
    let group = $('#filter_events_select_group').val();
    $('.table-event-spinner').removeClass('d-none');
    $.ajax({
        url: '/?handler=PrintEvents',
        method: "GET",
        data: { snCheckboxJson: JSON.stringify(checkboxJson), workerId: workerId, fio: fio, group: group, dateBeg: dateBeg, dateEnd: dateEnd },
        success: function (data) {
            $('.table-event-spinner').addClass('d-none');
            console.log(data);
            var link = document.createElement('a');
            link.setAttribute('href', data);
            link.setAttribute('target', '_blank');
            link.click();
        }
    });
}


// Задание имени контроллера
function SetControllerName(controllerId, name) {
    $.ajax({
        url: '/?handler=SetControllerName',
        method: "GET",
        data: { id: controllerId, name: name },
        success: function (data) {
            if (data == 'true') {
                alert("success");
            }
            else {
                alert(data);                
            }
            location.reload()
        }
    });
}


// Открытие модального окна для редактирования сотрудника
$('.edit_worker').on('click', function () {
    let workerId = $(this).attr('worker-id')
    $.ajax({
        url: `/?handler=CarPartial&id=${workerId}`,
        method: "GET",
        success: function (html) {
            console.log($(html).find('.modal-content').html())
            $('#Modal_worker_edit .modal-dialog').html(html);
        },
        error: (res) => {
            console.log(res)
        }
    });
});


// Открытие модального окна для добавления сотрудника
$('.add_worker').on('click', function () {
    $.ajax({
        url: `/?handler=CarPartial`,
        method: "GET",
        success: function (html) {
            console.log($(html).find('.modal-content').html())
            $('#Modal_worker_edit .modal-dialog').html(html);
        },
        error: (res) => {
            console.log(res)
        }
    });
});


// Смена типа доступа (Переключение радиокнопок)
$('.radio_access').on('click', function () {
    let workerId = $(this).attr('worker-id')
    let personalCheck = $(this).attr('value')
    $.ajax({
        url: '/?handler=ChangePersonalCheck',
        method: "GET",
        data: { workerId: workerId, personalCheck: personalCheck }
    });
});
