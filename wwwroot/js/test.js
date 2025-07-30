$(document).ready(function () {
    fetchTimesheetData();
});
// Handle opening the modal
$("#openmodal").click(function () {
    $("#exampleModal").modal("show");
});

$("#savebtn").click(function () {
    var obj = $("#myform").serialize();
    $.ajax({
        url: '/Timesheet/AddTimesheet',
        type: 'POST',
        contentType: 'application/x-www-form-urlencoded; charset=utf-8;',
        data: obj,
        dataType: 'json',
        success: function () {
            alert("Timesheet added successfully!");
            fetchTimesheetData();
            $("#exampleModal").modal("hide");
        },
        error: function () {
            alert("Something went wrong!!!");
        }
    });
});

$("#inp").on("input", function () {
    var data = $("#inp").val();
    $.ajax({
        url: "/Timesheet/Searching?mydata=" + data,
        type: "GET",
        success: function (result) {
            populateTable(result);
        }
    });
});

$("#opt").on("change", function () {
    //var data = $("#opt").val();
    //$.ajax({
    //    url: "/Timesheet/Sorting?mydata=" + data,
    //    type: "GET",
    //    success: function (result) {
    //        populateTable(result);
    //    }
    //});

    var selectedValue = $(this).val();

    if (selectedValue === "weekly") {
        fetchWeeklyTimesheets();
    } else if (selectedValue === "monthly") {
        fetchMonthlyTimesheets();
    } else if (selectedValue === "recent") {
        fetchRecentlyAddedTimesheets();
    } else if (selectedValue === "all") {
        fetchTimesheetData();
    } else {
        fetchSortedTimesheets(selectedValue);
    }
});

function fetchSortedTimesheets(order) {
    $.ajax({
        url: "/Timesheet/Sorting?mydata=" + order,
        type: "GET",
        success: function (result) {
            populateTable(result);
        }
    });
}

// Fetch last 7 days timesheets
function fetchWeeklyTimesheets() {
    $.ajax({
        url: "/Timesheet/FetchWeeklyTimesheets",
        type: "GET",
        success: function (result) {
            populateTable(result);
        }
    });
}

// Fetch last month's timesheets
function fetchMonthlyTimesheets() {
    $.ajax({
        url: "/Timesheet/FetchMonthlyTimesheets",
        type: "GET",
        success: function (result) {
            populateTable(result);
        }
    });
}

// Fetch recently added timesheets
function fetchRecentlyAddedTimesheets() {
    $.ajax({
        url: "/Timesheet/FetchRecentlyAddedTimesheets",
        type: "GET",
        success: function (result) {
            populateTable(result);
        }
    });
}


let timesheetData = []; // Stores fetched data
let currentPage = 1;
let rowsPerPage = 10; // Default entries per page

fetchTimesheetData();

$("#entriesPerPage").on("change", function () {
    rowsPerPage = parseInt($(this).val());
    currentPage = 1; // Reset to first page
    displayPage(currentPage);
    setupPagination();
});

function fetchTimesheetData() {
    $.ajax({
        url: "/Timesheet/FetchTimesheet",
        type: "GET",
        success: function (result) {
            timesheetData = result;
            displayPage(currentPage);
            setupPagination();
        }
    });
}

function displayPage(page) {
    currentPage = page;
    let start = (page - 1) * rowsPerPage;
    let end = start + rowsPerPage;
    let paginatedItems = timesheetData.slice(start, end);

    let obj = "";
    $.each(paginatedItems, function (index, item) {
        let profilePicture = item.user?.profilePicture
            ? /Content/uploads / ${ item.user.profilePicture.split('/').pop()
    }
                : "/assets/img/users/default.jpg";
    let fullName = item.user ? ${ item.user.firstName || ""
} ${ item.user.lastName || "" } : "N/A";

let statusBadge = <span class="badge bg-warning d-inline-flex align-items-center">${item.status}</span>;
if (item.status === "Approved") {
    statusBadge = <span class="badge bg-success d-inline-flex align-items-center">${item.status}</span>;
} else if (item.status === "Rejected") {
    statusBadge = <span class="badge bg-danger d-inline-flex align-items-center">${item.status}</span>;
}

obj += `<tr data-id="${item.timesheetId}">
                    <td><input type="checkbox" class="rowCheckbox" /></td>
                    <td>
                        <div class="d-flex align-items-center">
                            <a href="#" class="avatar avatar-md border avatar-rounded">
                                <img src="${profilePicture}" class="img-fluid" alt="User Image">
                            </a>
                            <div class="ms-2">
                                <h6 class="fw-medium"><a href="#">${fullName}</a></h6>
                            </div>
                        </div>
                    </td>
                    <td>${item.projects ? item.projects.projectName : "N/A"}</td>
                    <td>${formatDate(item.date)}</td>
                    <td>${item.workHours}</td>
                    <td>${statusBadge}</td>
                </tr>`;
});

$("#mydata").html(obj);
    }

function setupPagination() {
    let pageCount = Math.ceil(timesheetData.length / rowsPerPage);
    let paginationHTML = "";

    if (pageCount <= 1) {
        $("#pagination").html("");
        return; // Hide pagination if only one page
    }

    paginationHTML += `<li class="page-item ${currentPage === 1 ? 'disabled' : ''}">
                            <a class="page-link" href="javascript:void(0);" onclick="changePage(${currentPage - 1})">Previous</a>
                           </li>`;

    for (let i = 1; i <= pageCount; i++) {
        paginationHTML += `<li class="page-item ${i === currentPage ? 'active' : ''}">
                                <a class="page-link" href="javascript:void(0);" onclick="changePage(${i})">${i}</a>
                               </li>`;
    }

    paginationHTML += `<li class="page-item ${currentPage === pageCount ? 'disabled' : ''}">
                            <a class="page-link" href="javascript:void(0);" onclick="changePage(${currentPage + 1})">Next</a>
                           </li>`;

    $("#pagination").html(paginationHTML);
}

window.changePage = function (page) {
    if (page < 1 || page > Math.ceil(timesheetData.length / rowsPerPage)) return;
    displayPage(page);
    setupPagination();
};

function populateTable(data)
{
    let obj = "";
    $.each(data, function (index, item) {
        let profilePicture = item.user && item.user.profilePicture
            ? /Content/uploads / ${ item.user.profilePicture.split('/').pop()
    } // Ensure correct URL
                : "/assets/img/users/default.jpg";
    let fullName = item.user ? (item.user.firstName || "") + " " + (item.user.lastName || "") : "N/A";

    let statusBadge = <span class="badge bg-warning d-inline-flex align-items-center">${item.status}</span>;
    if (item.status === "Approved") {
        statusBadge = <span class="badge bg-success d-inline-flex align-items-center">${item.status}</span>;
    } else if (item.status === "Rejected") {
        statusBadge = <span class="badge bg-danger d-inline-flex align-items-center">${item.status}</span>;
    }

    obj += `<tr data-id="${item.timesheetId}">
                    <td><input type="checkbox" class="rowCheckbox" /></td>
                    
                    <td>
                        <div class="d-flex align-items-center file-name-icon">
                            <a href="#" class="avatar avatar-md border avatar-rounded">
                                <img src="${profilePicture}" class="img-fluid" alt="User Image">
                            </a>
                            <div class="ms-2">
                                <h6 class="fw-medium"><a href="#">${fullName}</a></h6>
                            </div>
                        </div>
                    </td>

                    <td>${item.projects ? item.projects.projectName : "N/A"}</td>
                    <td>${formatDate(item.date)}</td>
                    <td>${item.workHours}</td>
                    <td>${statusBadge}</td>
                </tr>`;
});
    $("#mydata").html(obj);
    
}

function formatDate(date) {
    const d = new Date(date);
    return d.toLocaleDateString("en-GB");
}

// Select/Deselect All Rows
$("#selectAll").click(function () {
    const isChecked = $(this).is(":checked");
    $("input[type='checkbox'].rowCheckbox").prop("checked", isChecked);
    toggleApprovalButton(); // Update button state
});

// Enable/Disable "Send for Approval" Button
function toggleApprovalButton() {
    const rows = $("input[type='checkbox'].rowCheckbox:checked").length;
    $("#sendForApproval").prop("disabled", rows === 0);
}

// Handle Individual Row Checkbox Click
$("#mydata").on("click", "input[type='checkbox'].rowCheckbox", function () {
    toggleApprovalButton();
    // Update "Select All" checkbox state
    const totalRows = $("input[type='checkbox'].rowCheckbox").length;
    const selectedRows = $("input[type='checkbox'].rowCheckbox:checked").length;
    $("#selectAll").prop("checked", totalRows > 0 && totalRows === selectedRows);
});

// Event delegation to handle dynamically generated checkboxes
$("#mydata").on("click", "input[type='checkbox']", function () {
    toggleApprovalButton();
});

function getSelectedTimesheetIds() {
    const selectedIds = [];
    $("input[type='checkbox'].rowCheckbox:checked").each(function () {
        const timesheetId = $(this).closest("tr").data("id");
        if (timesheetId) {
            selectedIds.push(timesheetId);
        }
    });
    return selectedIds;
}

$("#sendForApproval").click(function (e) {
    e.preventDefault(); // Prevent default form submission

    const selectedIds = getSelectedTimesheetIds(); // Collect selected IDs
    if (selectedIds.length > 0) {
        $("#timesheetIds").val(selectedIds.join(",")); // Set hidden input with comma-separated IDs
        $("#approvalForm").submit(); // Ensure the form is submitted
    } else {
        alert("No timesheets selected.");
    }
});

$("#exportPdf").click(function () {
    window.location.href = "/Timesheet/ExportToPdf";
});

$("#exportExcel").click(function () {
    window.location.href = "/Timesheet/ExportToExcel";
});
