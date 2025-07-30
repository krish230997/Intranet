$(document).ready(function () {
    fetchRole();
    $("#openmod").click(function () {
        console.log("Modal Trigger Clicked");
        $("#exampleModal").modal("show");
    });
});

//start with designation model
$("#savebtn1").click(function () {
    var isValid = true; // Validation flag

    // Validate required input fields and textareas
    $("#myRoleform input[required], #myRoleform textarea[required]").each(function () {
        if ($(this).val().trim() === "") {
            $(this).addClass("is-invalid");
            $(this).next(".error-msg").text("This field is required").show();
            isValid = false;
        } else {
            $(this).removeClass("is-invalid");
            $(this).next(".error-msg").hide();
        }
    });

    // Validate dropdown selection (Status)
    var statusValue = $("#Status").val();
    if (!statusValue || statusValue === "") {
        $("#Status").addClass("is-invalid");
        // Add the error message below the dropdown if it's not already there
        if ($("#Status").next(".error-msg").length === 0) {
            $("#Status").after('<div class="error-msg text-danger">Please select a valid status</div>');
        } else {
            $("#Status").next(".error-msg").show(); // Show the existing error message
        }
        isValid = false;
    } else {
        $("#Status").removeClass("is-invalid");
        $("#Status").next(".error-msg").hide(); // Hide the error message
    }

    // Stop form submission if validation fails
    if (!isValid) return;

    var obj = $("#myRoleform").serialize();

    $.ajax({
        url: '/Employee/AddRole',
        type: 'POST',
        contentType: 'application/x-www-form-urlencoded; charset=utf8',
        data: obj,
        dataType: 'json',
        success: function () {
            alert("Role Added Successfully");
            $('#exampleModal').modal('hide'); // Close the modal
            fetchRole();
            location.reload();
        },
        error: function () {
            alert("Something went wrong");
        }
    });
});
function fetchRole() {
    $.ajax({
        url: '/Employee/FetchRole',
        type: 'Get',
        contentType: 'application/json; charset=utf8',
        dataType: 'json',
        success: function (result, status, xhr) {
            console.log(result); // Add this to see the data 
            var obj = '';
            $.each(result, function (index, item) {
                console.log(item);
                let statusBadge = '';

                if (item.status === "Active") {
                    statusBadge = `<span class="badge bg-success"><i class="ti ti-point-filled me-1"></i>${item.status}</span>`; // Green badge
                } else if (item.status === "Inactive") {
                    statusBadge = `<span class="badge bg-danger"><i class="ti ti-point-filled me-1"></i>${item.status}</span>`; // Red badge
                } else {
                    statusBadge = `<span class="badge bg-secondary"><i class="ti ti-point-filled me-1"></i>${item.status}</span>`; // Default gray badge
                }

                obj += "<tr>";
                obj += "<td>" + item.roleId + "</td>";
                obj += "<td>" + item.roleName + "</td>";
                obj += "<td>" + statusBadge + "</td>"; // Use the status badge here
                obj += "<td>" + item.createdBy + "</td>";
                // obj += "<td>" + item.createdAt + "</td>";
                obj += "<td>" + item.modifiedBy + "</td>";
                //obj += "<td>" + item.modifiedAt + "</td>";
                obj += "<td>";
                obj += "<a href='#' class='edit-btn' data-id='" + item.roleId + "'><i class='ti ti-edit'></i></a> ";
                obj += "<a href='#' class='delete-btn' data-id='" + item.roleId + "'><i class='bi bi-trash'></i></a>";
                obj += "</td>";
                obj += "</tr>";
            });


            $("#mydata").html(obj);


            // Debug: Check if DataTables is loaded
            console.log($.fn.DataTable);

            // Destroy existing DataTable instance if it exists
            if ($.fn.DataTable.isDataTable("#table")) {
                $("#table").DataTable().destroy();
            }

            // Reinitialize DataTable after data is loaded
            $("#table").DataTable({
                "paging": true,        // Enable pagination
                "pageLength": 5,       // Default rows per page
                "lengthMenu": [5, 10, 25, 50, 100], // Dropdown for selecting rows per page
                "ordering": true,      // Enable column sorting
                "searching": true,     // Enable search box
                "lengthChange": true,
                "info": true,          // Show table info
                "responsive": true,    // Make table responsive
                "autoWidth": false,     // Disable auto column width
            });

            attachEventHandlers();
        },
        error: function () {
            alert("Something went wrong");
        }
    })
}
//function attachEventHandlers() {
//    $(".edit-btn").click(function () {
//        var id = $(this).data("id");
//        fetchRoleDetails(id);         //fetch detail and show edit model
//    });

//    $(".delete-btn").click(function () {
//        var id = $(this).data("id");
//        if (confirm("Are you sure you want to delete this Role?")) {
//            deleteRole(id);
//        }
//    });
//}

function attachEventHandlers() {
    // Use event delegation for edit buttons
    $('#table').on('click', '.edit-btn', function (e) {
        e.preventDefault(); // Prevent default action (e.g., following a link)
        var id = $(this).data("id");
        fetchRoleDetails(id); // Fetch details and show edit modal
    });

    // Use event delegation for delete buttons
    $('#table').on('click', '.delete-btn', function (e) {
        e.preventDefault(); // Prevent default action (e.g., following a link)
        var id = $(this).data("id");
        if (confirm("Are you sure you want to delete this Role?")) {
            deleteRole(id);
        }
    });
}




function fetchRoleDetails(id) {
    console.log("Fetching role details for ID:", id);  // Debugging log

    $.ajax({
        url: '/Employee/EditRole?eid=' + id,
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            console.log("Response received:", response); // Debugging log

            if (response.success) {
                var desi = response.data;

                $("#editRoleId").val(desi.roleId);
                $("#editRoleName").val(desi.roleName);

                // Populate status dropdown
                var statusDropdown = $("#editStatus");
                statusDropdown.empty();
                statusDropdown.append(`<option value="${desi.status}" selected>${desi.status}</option>`);

                $.each(response.statuses, function (i, status) {
                    if (status !== desi.status) {
                        statusDropdown.append(`<option value="${status}">${status}</option>`);
                    }
                });

                $("#editModal").modal("show");
            } else {
                console.error("Error:", response.message);
                alert(response.message);
            }
        },
        error: function (xhr, status, error) {
            console.error("AJAX Error:", status, error); // Log AJAX errors
            alert("Failed to fetch role details.");
        }
    });
}




// Save Edited Department
$("#saveEdit").click(function () {
    var obj = $("#editForm").serialize();
    $.ajax({
        url: '/Employee/EditRole',
        type: 'POST',
        data: obj,
        success: function () {
            alert("Role updated successfully");
            $("#editModal").modal("hide");
            fetchRole();
            location.reload();
        },
        error: function () {
            alert("Failed to update department");
        }
    });
});

// Delete Role
//function deleteRole(id) {
//    $.ajax({
//        url: '/Employee/DeleteRole',
//        type: 'POST',
//        data: { id: id },
//        success: function (response) {
//            if (response.success) {
//                alert(resonse.message);
//                fetchRole();
//                location.reload();
//            } else {
//                alert(response.message)
//            }
//        },
//        error: function () {
//            alert("Failed to delete department");
//        }
//    });
//}

function deleteRole(id) {
    $.ajax({
        url: '/Employee/DeleteRole',
        type: 'POST',
        data: { id: id },
        success: function (response) {

            console.log(response)
            if (response.success) {
                alert("Role deleted successfully");
                fetchRole();
                location.reload();
            }
            //else {
            //    alert(response.message);  // Show server error message
            //}
        },
        error: function (xhr, status, error) {
            var errorMessage = xhr.responseText ? JSON.parse(xhr.responseText).message : "An unexpected error occurred.";
            alert("Error: " + errorMessage);
        }
    });
}


$("#exportBtn").click(function () {
    $("#exportModal").modal('show');
});

$("#exportToPDF").click(function () {
    const { jsPDF } = window.jspdf;
    const doc = new jsPDF();

    // Get the table headers
    var headers = [];
    $('#mydata').closest('table').find('thead th').each(function () {
        headers.push($(this).text());
    });

    // Get the table rows
    var rows = [];
    $('#mydata tr').each(function () {
        var row = [];
        $(this).find('td').each(function () {
            row.push($(this).text());
        });
        if (row.length > 0) {
            rows.push(row);
        }
    });

    // Generate the table in the PDF
    doc.autoTable({
        head: [headers], // Headers
        body: rows,      // Data rows
    });

    // Save the PDF
    doc.save('role.pdf');
});

//$("#exportToExcel").click(function () {
//    var wb = XLSX.utils.book_new();
//    var ws = XLSX.utils.table_to_sheet($('#mydata')[0]);


//    // Add sheet to the workbook
//    XLSX.utils.book_append_sheet(wb, ws, "Departments");

//    // Download the Excel file
//    XLSX.writeFile(wb, 'role.xlsx');
//});
$("#exportToExcel").click(function () {
    // Store the last column elements
    var lastTh = $('#table th:last-child').detach();
    var lastTds = $('#table td:last-child').detach();

    var wb = XLSX.utils.book_new();
    var ws = XLSX.utils.table_to_sheet($('#table')[0]);

    // Add sheet to the workbook
    XLSX.utils.book_append_sheet(wb, ws, "Roles");

    // Download the Excel file
    XLSX.writeFile(wb, 'role.xlsx');

    // Restore the last column
    $('#table thead tr').append(lastTh);
    $('#table tbody tr').each(function (index, row) {
        $(row).append(lastTds[index]);
    });
});

//starrt with filtering
$("#opt").on('click', function () {
    var data = $("#opt").val();
    $.ajax({
        url: '/Employee/RoleSorting?mydata=' + data,
        type: 'Get',
        contentType: 'application/json; charset=utf8',
        dataType: 'json',
        success: function (result, status, xhr) {
            console.log(result); // Add this to see the data returned
            var obj = '';
            $.each(result, function (index, item) {
                console.log(item);
                let statusBadge = '';

                if (item.status === "Active") {
                    statusBadge = `<span class="badge bg-success"><i class="ti ti-point-filled me-1"></i>${item.status}</span>`; // Green badge
                } else if (item.status === "Inactive") {
                    statusBadge = `<span class="badge bg-danger"><i class="ti ti-point-filled me-1"></i>${item.status}</span>`; // Red badge
                } else {
                    statusBadge = `<span class="badge bg-secondary"><i class="ti ti-point-filled me-1"></i>${item.status}</span>`; // Default gray badge
                }

                obj += "<tr>";
                obj += "<td>" + item.roleId + "</td>";
                obj += "<td>" + item.roleName + "</td>";
                obj += "<td>" + statusBadge + "</td>"; // Use the status badge here
                obj += "<td>" + item.createdBy + "</td>";
                // obj += "<td>" + item.createdAt + "</td>";
                obj += "<td>" + item.modifiedBy + "</td>";
                //obj += "<td>" + item.modifiedAt + "</td>";
                obj += "<td>";
                obj += "<a href='#' class='edit-btn' data-id='" + item.roleId + "'><i class='ti ti-edit'></i></a> ";
                obj += "<a href='#' class='delete-btn' data-id='" + item.roleId + "'><i class='bi bi-trash'></i></a>";
                obj += "</td>";
                obj += "</tr>";
            });
            $("#mydata").html(obj);

            attachEventHandlers();  // Reattach event handlers for edit and delete buttons
        },
        error: function () {
            alert("Something went wrong");
        }
    })
})


$("#sts").on('change', function () {
    var data = $(this).val(); // Get the selected value from the dropdown
    $.ajax({
        url: '/Employee/RoleSortingStatus?mydata=' + data, // Call the SortingStatus endpoint
        type: 'GET',
        contentType: 'application/json; charset=utf8',
        dataType: 'json',
        success: function (result) {
            var obj = '';
            $.each(result, function (index, item) {
                console.log(item);
                let statusBadge = '';

                if (item.status === "Active") {
                    statusBadge = `<span class="badge bg-success"><i class="ti ti-point-filled me-1"></i>${item.status}</span>`; // Green badge
                } else if (item.status === "Inactive") {
                    statusBadge = `<span class="badge bg-danger"><i class="ti ti-point-filled me-1"></i>${item.status}</span>`; // Red badge
                } else {
                    statusBadge = `<span class="badge bg-secondary"><i class="ti ti-point-filled me-1"></i>${item.status}</span>`; // Default gray badge
                }

                obj += "<tr>";
                obj += "<td>" + item.roleId + "</td>";
                obj += "<td>" + item.roleName + "</td>";
                obj += "<td>" + statusBadge + "</td>"; // Use the status badge here
                obj += "<td>" + item.createdBy + "</td>";
                // obj += "<td>" + item.createdAt + "</td>";
                obj += "<td>" + item.modifiedBy + "</td>";
                //obj += "<td>" + item.modifiedAt + "</td>";
                obj += "<td>";
                obj += "<a href='#' class='edit-btn' data-id='" + item.roleId + "'><i class='ti ti-edit'></i></a> ";
                obj += "<a href='#' class='delete-btn' data-id='" + item.roleId + "'><i class='bi bi-trash'></i></a>";
                obj += "</td>";
                obj += "</tr>";
            });
            $("#mydata").html(obj); // Update the table body with the filtered data
            attachEventHandlers();  // Reattach event handlers for edit and delete buttons
        },
        error: function () {
            alert("Failed to filter departments by status.");
        }
    });
});


///search query
$("#inp").on('input', function () {
    var data = $("#inp").val(); // Get the value from the search box
    $.ajax({
        url: '/Employee/SearchRole?mydata=' + data, // Update with your search API endpoint
        type: 'GET', // Use GET to fetch search results
        contentType: 'application/json; charset=utf8', // Set the content type
        dataType: 'json', // Expect JSON response
        success: function (result, status, xhr) {
            console.log(result); // Add this to see the data returned
            var obj = '';
            $.each(result, function (index, item) {
                console.log(item);
                let statusBadge = '';

                if (item.status === "Active") {
                    statusBadge = `<span class="badge bg-success"><i class="ti ti-point-filled me-1"></i>${item.status}</span>`; // Green badge
                } else if (item.status === "Inactive") {
                    statusBadge = `<span class="badge bg-danger"><i class="ti ti-point-filled me-1"></i>${item.status}</span>`; // Red badge
                } else {
                    statusBadge = `<span class="badge bg-secondary"><i class="ti ti-point-filled me-1"></i>${item.status}</span>`; // Default gray badge
                }

                obj += "<tr>";
                obj += "<td>" + item.roleId + "</td>";
                obj += "<td>" + item.roleName + "</td>";
                obj += "<td>" + statusBadge + "</td>"; // Use the status badge here
                obj += "<td>" + item.createdBy + "</td>";
                // obj += "<td>" + item.createdAt + "</td>";
                obj += "<td>" + item.modifiedBy + "</td>";
                //obj += "<td>" + item.modifiedAt + "</td>";
                obj += "<td>";
                obj += "<a href='#' class='edit-btn' data-id='" + item.roleId + "'><i class='ti ti-edit'></i></a> ";
                obj += "<a href='#' class='delete-btn' data-id='" + item.roleId + "'><i class='bi bi-trash'></i></a>";
                obj += "</td>";
                obj += "</tr>";
            });
            $("#mydata").html(obj); // Replace the table body with the new rows
            attachEventHandlers();  // Reattach event handlers for edit and delete buttons
        },
        error: function () {
            alert("Something went wrong"); // Alert on error
        }
    });
});
//no of rows
$("#rows").change(function () {
    var rowCount = $(this).val(); // Get the selected value

    if (rowCount != "0") { // Ensure a valid value is selected
        $.ajax({
            url: "/Employee/GetRoleEntries", // Replace with your action endpoint
            type: "GET",
            data: { count: rowCount }, // Pass the selected value to the server
            success: function (response) {
                var tableBody = $("#table tbody");
                tableBody.empty(); // Clear existing table rows

                // Iterate over the response and append rows
                response.forEach(function (item, index) {
                    tableBody.append(
                        `<tr>
                                <td>${index + 1}</td>
                                <td>${item.roleName}</td>
                                <td><span class="badge bg-success"><i class="ti ti-point-filled me-1"></i>${item.Status}</span></td>
                                                obj += "<td>" + item.createdBy + "</td>";
               // obj += "<td>" + item.createdAt + "</td>";
                obj += "<td>" + item.modifiedBy + "</td>";
                //obj += "<td>" + item.modifiedAt + "</td>";
                             </tr>`
                    );
                });
            },
            error: function (error) {
                console.error("Error fetching data: ", error);
            }
        });
    } else {
        $("#table tbody").empty(); // Clear table if "--Select--" is chosen
    }
});