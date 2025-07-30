$(document).ready(function () {
    fetchDesignation();
    $("#openmod").click(function () {
        console.log("Modal Trigger Clicked");
        $("#exampleModal").modal("show");
    });
});

//start with designation model
$("#savebtn1").click(function () {
    var isValid = true; // Validation flag

    // Validate required input fields (Designation Name)
    $("#mydesignationform input[required]").each(function () {
        if ($(this).val().trim() === "") {
            $(this).addClass("is-invalid");
            // Add error message below the input if not already there
            if ($(this).next(".error-msg").length === 0) {
                $(this).after('<div class="error-msg text-danger">This field is required</div>');
            } else {
                $(this).next(".error-msg").show();
            }
            isValid = false;
        } else {
            $(this).removeClass("is-invalid");
            $(this).next(".error-msg").hide();
        }
    });

    // Validate Department selection
    var departmentValue = $("select[name='DepartmentId']").val();
    if (!departmentValue || departmentValue === "") {
        $("select[name='DepartmentId']").addClass("is-invalid");
        // Show error message below dropdown
        if ($("select[name='DepartmentId']").next(".error-msg").length === 0) {
            $("select[name='DepartmentId']").after('<div class="error-msg text-danger">Please select a department</div>');
        } else {
            $("select[name='DepartmentId']").next(".error-msg").show();
        }
        isValid = false;
    } else {
        $("select[name='DepartmentId']").removeClass("is-invalid");
        $("select[name='DepartmentId']").next(".error-msg").hide();
    }

    // Validate Status selection
    var statusValue = $("select[name='Status']").val();
    if (!statusValue || statusValue === "Select") {
        $("select[name='Status']").addClass("is-invalid");
        // Show error message below dropdown
        if ($("select[name='Status']").next(".error-msg").length === 0) {
            $("select[name='Status']").after('<div class="error-msg text-danger">Please select a valid status</div>');
        } else {
            $("select[name='Status']").next(".error-msg").show();
        }
        isValid = false;
    } else {
        $("select[name='Status']").removeClass("is-invalid");
        $("select[name='Status']").next(".error-msg").hide();
    }

    // Stop form submission if validation fails
    if (!isValid) return;

    var obj = $("#mydesignationform").serialize();

    $.ajax({
        url: '/Employee/AddDesignation',
        type: 'POST',
        contentType: 'application/x-www-form-urlencoded; charset=utf8',
        data: obj,
        dataType: 'json',
        success: function () {
            alert("Designation Added Successfully");
            $('#exampleModal').modal('hide'); // Close the modal
            fetchDesignation();

            location.reload();
        },
        error: function () {
            alert("Something went wrong");
        }
    });
});
//function fetchDesignation() {
//    $.ajax({
//        url: '/Employee/FetchDesignation',
//        type: 'Get',
//        contentType: 'application/json; charset=utf8',
//        dataType: 'json',
//        success: function (result, status, xhr) {
//            console.log(item); // Add this to see the data returned
//            let statusBadge = '';
//            if (item.status === "Active") {
//                statusBadge = '<span class="badge bg-success"><i class="ti ti-point-filled me-1"></i>' + item.status + '</span>'; // Green badge
//            } else {
//                statusBadge = '<span class="badge bg-danger"><i class="ti ti-point-filled me-1"></i>' + item.status + '</span>'; // Red badge
//            }
//            var obj = '';
//            $.each(result, function (index, item) {
//                console.log(item);
//                obj += "<tr>";
//                obj += "<td>" + item.designationId + "</td>";

//                obj += "<td>" + (item.name ? item.name : "N/A") + "</td>"; // Handle undefined case
//                obj += "<td>" + (item.department && item.department.name ? item.department.name : "N/A") + "</td>"; // Correct way to access nested object

//                obj += "<td>" + item.noOfEmployee + "</td>";
//                obj += '<td><span class="badge bg-success"><i class="ti ti-point-filled me-1"></i>' + statusBadge + "</span></td>"
//                obj += "<td>" + item.createdBy + "</td>";
//                // obj += "<td>" + item.createdAt + "</td>";
//                obj += "<td>" + item.modifiedBy + "</td>";
//                //obj += "<td>" + item.modifiedAt + "</td>";
//                obj += "<td>";
//                obj += "<a href='#' class='edit-btn' data-id='" + item.designationId + "'><i class='ti ti-edit'></i></a> ";
//                obj += "<a href='#' class='delete-btn' data-id='" + item.designationId + "'><i class='bi bi-trash'></i></a>";
//                obj += "</td>";
//                obj += "</tr>";
//            });
//            $("#mydata").html(obj);

//            // Debug: Check if DataTables is loaded
//            console.log($.fn.DataTable);

//            // Destroy existing DataTable instance if it exists
//            if ($.fn.DataTable.isDataTable("#table")) {
//                $("#table").DataTable().destroy();
//            }

//            // Reinitialize DataTable after data is loaded
//            $("#table").DataTable({
//                "paging": true,        // Enable pagination
//                "pageLength": 10,       // Default rows per page
//                "lengthMenu": [10, 25, 50, 100], // Dropdown for selecting rows per page
//                "ordering": true,      // Enable column sorting
//                "searching": true,     // Enable search box

//                "info": true,          // Show table info
//                "responsive": true,    // Make table responsive
//                "autoWidth": false,     // Disable auto column width

//            });

//            attachEventHandlers();
//        },
//        error: function () {
//            alert("Something went wrong");
//        }
//    })
//}

function fetchDesignation() {
    $.ajax({
        url: '/Employee/FetchDesignation',
        type: 'GET',
        contentType: 'application/json; charset=utf8',
        dataType: 'json',
        success: function (result, status, xhr) {
            console.log(result); // Debugging to check the response

            var obj = '';

            $.each(result, function (index, item) {
                console.log(item); // Debugging each item

                // Define statusBadge inside the loop
                let statusBadge = '';
                if (item.status === "Active") {
                    statusBadge = '<span class="badge bg-success"><i class="ti ti-point-filled me-1"></i>' + item.status + '</span>'; // Green badge
                } else {
                    statusBadge = '<span class="badge bg-danger"><i class="ti ti-point-filled me-1"></i>' + item.status + '</span>'; // Red badge
                }

                obj += "<tr>";
                obj += "<td>" + item.designationId + "</td>";
                obj += "<td>" + (item.name ? item.name : "N/A") + "</td>"; // Handle undefined case
                obj += "<td>" + (item.department && item.department.name ? item.department.name : "N/A") + "</td>"; // Handle nested object safely
                obj += "<td>" + item.noOfEmployee + "</td>";
                obj += "<td>" + statusBadge + "</td>"; // Corrected status badge placement
                obj += "<td>" + item.createdBy + "</td>";
                obj += "<td>" + item.modifiedBy + "</td>";
                obj += "<td>";
                obj += "<a href='#' class='edit-btn' data-id='" + item.designationId + "'><i class='ti ti-edit'></i></a> ";
                obj += "<a href='#' class='delete-btn' data-id='" + item.designationId + "'><i class='bi bi-trash'></i></a>";
                obj += "</td>";
                obj += "</tr>";
            });

            $("#mydata").html(obj);

            // Debugging: Check if DataTables is loaded
            console.log($.fn.DataTable);

            // Destroy existing DataTable instance if it exists
            if ($.fn.DataTable.isDataTable("#table")) {
                $("#table").DataTable().destroy();
            }

            // Reinitialize DataTable after data is loaded
            $("#table").DataTable({
                "paging": true,
                "pageLength": 5,
                "lengthMenu": [5, 10, 25, 50, 100],
                "ordering": true,
                "searching": true,
                "info": true,
                "responsive": true,
                "autoWidth": false
            });

            attachEventHandlers();
        },
        error: function () {
            alert("Something went wrong");
        }
    });
}

//function attachEventHandlers() {
//    $(".edit-btn").click(function () {
//        var id = $(this).data("id");
//        fetchDesignationDetails(id);         //fetch detail and show edit model
//    });

//    $(".delete-btn").click(function () {
//        var id = $(this).data("id");
//        if (confirm("Are you sure you want to delete this department?")) {
//            deleteDesignation(id);
//        }
//    });
//}

function attachEventHandlers() {
    // Use event delegation for edit buttons
    $('#table').on('click', '.edit-btn', function (e) {
        e.preventDefault(); // Prevent default action (e.g., following a link)
        var id = $(this).data("id");
        fetchDesignationDetails(id); // Fetch details and show edit modal
    });

    // Use event delegation for delete buttons
    $('#table').on('click', '.delete-btn', function (e) {
        e.preventDefault(); // Prevent default action (e.g., following a link)
        var id = $(this).data("id");
        if (confirm("Are you sure you want to delete this Designation?")) {
            deleteDesignation(id);
        }
    });
}

// Fetch Department Details for Edit
function fetchDesignationDetails(id) {
    $.ajax({
        url: '/Employee/EditDesignation?eid=' + id,
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            if (response.success) {
                var desi = response.data;

                $("#DesignationId").val(desi.designationId);
                $("#editDesignationname").val(desi.name);
                //$("#editStatus").val(desi.status);

                // Populate department dropdown
                var departmentDropdown = $("#DepartmentId");
                departmentDropdown.empty(); // Clear previous options

                $.each(response.departments, function (i, department) {
                    var selected = (department.departmentId == desi.departmentId) ? "selected" : "";
                    departmentDropdown.append(`<option value="${department.departmentId}" ${selected}>${department.name}</option>`);
                });

                //populate status dropdown with logic
                var statusDropdown = $("#status");
                statusDropdown.empty();

                if (desi.status === "Active") {
                    statusDropdown.append(`<option value="Active" selected>Active</option>`);
                    statusDropdown.append(`<option value="Inactive">Inactive</option>`);
                } else if (desi.status === "Inactive") {
                    statusDropdown.append(`<option value="Inactive" selected>Inactive</option>`);
                    statusDropdown.append(`<option value="Active">Active</option>`);
                } else {
                    $.each(response.statuses, function (i, status) {
                        var selected = (status === desi.status) ? "selected" : "";
                        statusDropdown.append(`<option value="${status}" ${selected}>${status}</option>`);
                    });
                }


                $("#editModal").modal("show");

            } else {
                alert(response.message);
            }
        },
        error: function () {
            alert("Failed to fetch designation details.");
        }
    });
}


// Save Edited Department
$("#saveEdit").click(function () {
    var obj = {
        designationId: $("#DesignationId").val(),
        name: $("#editDesignationname").val(),
        departmentId: $("#DepartmentId").val(),
        status: $("#status").val()
    };

    $.ajax({
        url: '/Employee/EditDesignationDetails',
        type: 'POST',
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify(obj),
        success: function (response) {
            if (response.success) {
                alert("Designation updated successfully");
                $("#editModal").modal("hide");
                fetchDesignation();
                location.reload();
            } else {
                alert(response.message || "Failed to update designation.");
            }
        },
        error: function () {
            alert("An error occurred while updating the designation.");
        }
    });
});


// Delete Designation
//function deleteDesignation(id) {
//    $.ajax({
//        url: '/Employee/DeleteDesignation',
//        type: 'POST',
//        data: { id: id },
//        success: function (response) {
//            if (response.success) {
//                alert(response.message);
//                fetchDesignation();
//            } else {
//                alert(response.message);
//            }
//        },
//        error: function () {
//            alert("Failed to delete department");
//        }
//    });
//}

function deleteDesignation(id) {
    $.ajax({
        url: '/Employee/DeleteDesignation',
        type: 'POST',
        data: { id: id },
        success: function () {
            alert("Designation deleted successfully");
            fetchDesignation();
            location.reload();
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
    doc.save('departments.pdf');
});

//$("#exportToExcel").click(function () {
//    var wb = XLSX.utils.book_new();
//    var ws = XLSX.utils.table_to_sheet($('#mydata')[0]);


//    // Add sheet to the workbook
//    XLSX.utils.book_append_sheet(wb, ws, "Departments");

//    // Download the Excel file
//    XLSX.writeFile(wb, 'departments.xlsx');
//});
$("#exportToExcel").click(function () {
    // Store the last column elements
    var lastTh = $('#table th:last-child').detach();
    var lastTds = $('#table td:last-child').detach();

    var wb = XLSX.utils.book_new();
    var ws = XLSX.utils.table_to_sheet($('#table')[0]);

    // Add sheet to the workbook
    XLSX.utils.book_append_sheet(wb, ws, "Designation");

    // Download the Excel file
    XLSX.writeFile(wb, 'designation.xlsx');

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
        url: '/Employee/DesiSorting?mydata=' + data,
        type: 'Get',
        contentType: 'application/json; charset=utf8',
        dataType: 'json',
        success: function (result, status, xhr) {
            console.log(result); // Add this to see the data returned
            var obj = '';
            $.each(result, function (index, item) {
                console.log(item); // Debugging each item

                // Define statusBadge inside the loop
                let statusBadge = '';
                if (item.status === "Active") {
                    statusBadge = '<span class="badge bg-success"><i class="ti ti-point-filled me-1"></i>' + item.status + '</span>'; // Green badge
                } else {
                    statusBadge = '<span class="badge bg-danger"><i class="ti ti-point-filled me-1"></i>' + item.status + '</span>'; // Red badge
                }

                obj += "<tr>";
                obj += "<td>" + item.designationId + "</td>";
                obj += "<td>" + (item.name ? item.name : "N/A") + "</td>"; // Handle undefined case
                obj += "<td>" + (item.department && item.department.name ? item.department.name : "N/A") + "</td>"; // Handle nested object safely
                obj += "<td>" + item.noOfEmployee + "</td>";
                obj += "<td>" + statusBadge + "</td>"; // Corrected status badge placement
                obj += "<td>" + item.createdBy + "</td>";
                obj += "<td>" + item.modifiedBy + "</td>";
                obj += "<td>";
                obj += "<a href='#' class='edit-btn' data-id='" + item.designationId + "'><i class='ti ti-edit'></i></a> ";
                obj += "<a href='#' class='delete-btn' data-id='" + item.designationId + "'><i class='bi bi-trash'></i></a>";
                obj += "</td>";
                obj += "</tr>";
            });
            $("#mydata").html(obj);
        },
        error: function () {
            alert("Something went wrong");
        }
    })
})


$("#sts").on('change', function () {
    var data = $(this).val(); // Get the selected value from the dropdown
    $.ajax({
        url: '/Employee/DesiSortingStatus?mydata=' + data, // Call the SortingStatus endpoint
        type: 'GET',
        contentType: 'application/json; charset=utf8',
        dataType: 'json',
        success: function (result) {
            var obj = '';
            $.each(result, function (index, item) {
                console.log(item); // Debugging each item

                // Define statusBadge inside the loop
                let statusBadge = '';
                if (item.status === "Active") {
                    statusBadge = '<span class="badge bg-success"><i class="ti ti-point-filled me-1"></i>' + item.status + '</span>'; // Green badge
                } else {
                    statusBadge = '<span class="badge bg-danger"><i class="ti ti-point-filled me-1"></i>' + item.status + '</span>'; // Red badge
                }

                obj += "<tr>";
                obj += "<td>" + item.designationId + "</td>";
                obj += "<td>" + (item.name ? item.name : "N/A") + "</td>"; // Handle undefined case
                obj += "<td>" + (item.department && item.department.name ? item.department.name : "N/A") + "</td>"; // Handle nested object safely
                obj += "<td>" + item.noOfEmployee + "</td>";
                obj += "<td>" + statusBadge + "</td>"; // Corrected status badge placement
                obj += "<td>" + item.createdBy + "</td>";
                obj += "<td>" + item.modifiedBy + "</td>";
                obj += "<td>";
                obj += "<a href='#' class='edit-btn' data-id='" + item.designationId + "'><i class='ti ti-edit'></i></a> ";
                obj += "<a href='#' class='delete-btn' data-id='" + item.designationId + "'><i class='bi bi-trash'></i></a>";
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
        url: '/Employee/SearchDesi?mydata=' + data, // Update with your search API endpoint
        type: 'GET', // Use GET to fetch search results
        contentType: 'application/json; charset=utf8', // Set the content type
        dataType: 'json', // Expect JSON response
        success: function (result, status, xhr) {
            console.log(result); // Add this to see the data returned
            var obj = '';
            $.each(result, function (index, item) {
                console.log(item);
                obj += "<tr>";
                obj += "<td>" + item.designationId + "</td>";
                obj += "<td>" + item.name + "</td>";
                obj += "<td>" + (item.department && item.department.name ? item.department.name : "N/A") + "</td>"; // Correct way to access nested object
                obj += "<td>" + item.noOfEmployee + "</td>"
                obj += '<td><span class="badge bg-success"><i class="ti ti-point-filled me-1"></i>' + item.status + "</span></td>"
                obj += "<td>";
                obj += "<a href='#' class='edit-btn' data-id='" + item.designationId + "'><i class='ti ti-edit'></i></a> ";
                obj += "<a href='#' class='delete-btn' data-id='" + item.designationId + "'><i class='bi bi-trash'></i></a>";
                obj += "</td>";
                obj += "</tr>";
            });
            $("#mydata").html(obj); // Replace the table body with the new rows
        },
        error: function () {
            alert("Something went wrong"); // Alert on error
        }
    });
});



$("#rows").change(function () {
    var rowCount = $(this).val(); // Get the selected value as a number

    if (rowCount != "0") { // Ensure a valid number is selected
        $.ajax({
            url: "/Employee/GetEntries", // Replace with your action endpoint
            type: "GET",
            data: { count: rowCount }, // Pass the selected value to the server
            dataType: "json", // Ensure the response is treated as JSON
            success: function (response) {
                console.log("Response from server:", response);

                var tableBody = $("#table tbody");

                // Filter logic: Hide rows not matching the filtered count
                var existingRows = tableBody.find("tr");

                if (existingRows.length > rowCount) {
                    // Hide excess rows beyond the selected row count
                    existingRows.each(function (index) {
                        if (index >= rowCount) {
                            $(this).hide();
                        } else {
                            $(this).show();
                        }
                    });
                } else if (existingRows.length < rowCount) {
                    // Fetch missing rows from the server and append them
                    var missingCount = rowCount - existingRows.length;

                    response.slice(0, missingCount).forEach(function (item, index) {
                        tableBody.append(
                            `<tr>
                                <td>${item.designationId}</td>
                                <td>${item.name || "N/A"}</td>
                                <td>${item.Department || "N/A"}</td>
                                <td><span class="badge bg-success"><i class="ti ti-point-filled me-1"></i>${item.Status || "N/A"}</span></td>
                            </tr>`
                        );
                    });
                } else {
                    // Show all rows if they match the selected count
                    existingRows.show();
                }
            },
            error: function (error) {
                console.error("Error fetching data: ", error);
                alert("Unable to fetch data. Please try again later.");
            }
        });
    } else {

        $("#table tbody tr").show();
    }
});
