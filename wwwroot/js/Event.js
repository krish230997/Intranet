//$(document).ready(function () {

//});
//$("#btnmod").click(function () {
//    $("#addEventModal").modal('show');
//});


//$("#savemod").click(function () {
//    //var obj = {
//    //    name:$("#Name").val(),
//    //    email: $("#Email").val(),
//    //        dept: $("#Dept").val(),
//    //            salary: $("#Salary").val()
//    //}
//    var obj = $("#addEventForm").serialize();
//    $.ajax({
//        url: '/Event/AddEvent',
//        type: 'Post',
//        contentType: 'application/x-www-form-urlencoded;charset=utf8;',
//        data: obj,
//        dataType: 'json',
//        success: function () {
//            alert("Emp Added Suceessfully");


//        },
//        error: function () {
//            alert("something went wrong");
//        }

//    });

//});

//    $('#addEventForm').on('submit', function (e) {
//        e.preventDefault();

//        // Prevent multiple submissions
//        if ($(this).data('submitting')) return;
//        $(this).data('submitting', true);

//        // Get form values
//        const eventTitle = $('#eventTitle').val();
//        const eventDate = $('#eventDate').val();
//        const eventTypeId = $('#eventType').val();

//        // Send AJAX request
//        $.ajax({
//            url: '/Event/AddEvent',
//            method: 'POST',
//            contentType: 'application/json',
//            data: JSON.stringify({
//                Title: eventTitle,
//                Date: eventDate,
//                EventTypeId: parseInt(eventTypeId)
//            }),
//            success: function (response) {
//                // Check if the response is successful
//                if (response) {
//                    console.log('Event added successfully:', response);

//                    // Refetch events in the calendar
//                    if (typeof calendar.refetchEvents === 'function') {
//                        console.log('Refetching events...');
//                        calendar.refetchEvents(); // Refresh the calendar
//                    } else {
//                        console.error('calendar.refetchEvents is not a function');
//                    }

//                    // Reset the form
//                    $('#addEventForm')[0].reset();

//                    // Optional: Show a success message
//                    alert('Event added successfully!');
//                } else {
//                    alert('Failed to add event. Please try again.');
//                }
//            },
//            error: function (xhr, status, error) {
//                // Handle errors
//                console.error('Error:', error);
//                alert('An error occurred while adding the event. Please try again.');
//            },
//            complete: function () {
//                // Reset the submitting flag
//                $('#addEventForm').data('submitting', false);
//            }
//        });
//    });
//});

$(document).ready(function () {
    const $calendarEl = $('#calendar');
    const $eventListEl = $('#eventList');
    const $eventForm = $('#addEventForm');

    $('#btnmod').on('click', function () {
        $('#addEventModal').modal('show');
    });


    // Fetch event types and populate the dropdown
    $.getJSON('/Event/GetEventTypes', function (eventTypes) {
        const $select = $('#eventType');
        $.each(eventTypes, function (index, type) {
            $select.append($('<option>', {
                value: type.id,
                text: type.name
            }));
        });
    });

    // Initialize FullCalendar
    const calendar = new FullCalendar.Calendar($calendarEl[0], {
        initialView: 'dayGridMonth',
        headerToolbar: {
            left: 'prev,next today',
            center: 'title',
            right: 'dayGridMonth,timeGridWeek,timeGridDay',
        },
        events: function (fetchInfo, successCallback, failureCallback) {
            $.ajax({
                url: '/Event/GetEvents',
                method: 'GET',
                success: function (response) {
                    // Filter only active events before adding to calendar
                    let activeEvents = response.filter(event => event.status === "Active");

                    successCallback(activeEvents); // Load only active events
                },
                error: function () {
                    alert('There was an error fetching events!');
                    failureCallback();
                }
            });
        },
        eventDidMount: function (info) {
            // Apply custom styling to events
            if (info.event.extendedProps.color) {
                $(info.el).css('background-color', info.event.extendedProps.color);
            }
        },
        eventClick: function (info) {
            alert(`Event: ${info.event.title}\nDate: ${info.event.start.toLocaleDateString()}`);
        },
        eventContent: function (info) {
            return {
                html: `<div class="fc-event-main-content">
                <div class="fc-event-title">${info.event.title}</div>
                ${info.event.extendedProps.time ? `<div class="fc-event-time">${info.event.extendedProps.time}</div>` : ''}
            </div>`
            };
        }
    });

    calendar.render();


    // Function to refresh event list
    function refreshEventList() {
        $.getJSON('/Event/GetEvents', function (events) {
            $eventListEl.empty();
            $.each(events, function (index, event) {
                const $eventItem = $('<li>', { class: 'event-item' }).html(`
                    <h6>${event.title}</h6>
                    <div class="event-date">${event.start}</div>
                    <div class="event-description">${event.description || ''}</div>
                `);
                $eventListEl.append($eventItem);
            });
        });
    }
    refreshEventList();

    // Event form submission handler
    $('#addEventForm').on('submit', function (e) {
        e.preventDefault();
        if (this.submitting) return;
        this.submitting = true;

        let eventTitle = $('#eventTitle').val();
        let eventDate = $('#eventDate').val();
        let eventTypeId = $('#eventType').val();

        $.ajax({
            url: '/Event/AddEvent',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                Title: $('#eventTitle').val(),
                Date: $('#eventDate').val(),
                EventTypeId: parseInt($('#eventType').val()),
                Status: "Active" // Ensure default status
            }),
            success: function (newEvent) {
                $('#addEventModal').modal('hide');
                location.reload(); // Refresh calendar
            },
            error: function (xhr, status, error) {
                console.error('AJAX Error:', xhr.responseText);
                alert('Error: ' + xhr.responseText);
            }
        });
    });


});
