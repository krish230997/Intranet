$(document).ready(function () {
    fetchUser();
    $("#openmod").click(function () {
        console.log("Modal Trigger Clicked");
        $("#exampleModal").modal("show");
    });
});

//start with designation model
$("#savebtn1").click(function () {
    var isValid = true; // Validation flag

    // Validate required input fields (Designation Name)
    $("#myempform input[required]").each(function () {
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
    // Validate Profile Picture Input
    var profileInput = $("#ProfilePicture")[0]; // Get file input
    if (profileInput.files.length > 0) {
        var file = profileInput.files[0];
        var allowedExtensions = ["jpg", "jpeg", "png", "gif"];
        var fileExtension = file.name.split('.').pop().toLowerCase();
        var allowedTypes = ["image/jpeg", "image/png", "image/gif"];

        if (!allowedExtensions.includes(fileExtension) || !allowedTypes.includes(file.type)) {
            alert("Only image files (JPEG, PNG, GIF) are allowed.");
            isValid = false;
        }
    } else {
        // If no file is selected, show an error message below the input
        if ($("#ProfilePicture").next(".error-msg").length === 0) {
            $("#ProfilePicture").after('<div class="error-msg text-danger">Please select an image file.</div>');
        } else {
            $("#ProfilePicture").next(".error-msg").show();
        }
        isValid = false;
    }


    // Validate Department selection
    //var departmentValue = $("select[name='DepartmentId']").val();
    //if (!departmentValue || departmentValue === "") {
    //    $("select[name='DepartmentId']").addClass("is-invalid");
    //    // Show error message below dropdown
    //    if ($("select[name='DepartmentId']").next(".error-msg").length === 0) {
    //        $("select[name='DepartmentId']").after('<div class="error-msg text-danger">Please select a department</div>');
    //    } else {
    //        $("select[name='DepartmentId']").next(".error-msg").show();
    //    }
    //    isValid = false;
    //} else {
    //    $("select[name='DepartmentId']").removeClass("is-invalid");
    //    $("select[name='DepartmentId']").next(".error-msg").hide();
    //}
    ////designation
    //var designationValue = $("select[name='DesignationtId']").val();
    //if (!designationValue || designationValue === "") {
    //    $("select[name='DesignationtId']").addClass("is-invalid");
    //    // Show error message below dropdown
    //    if ($("select[name='DesignationtId']").next(".error-msg").length === 0) {
    //        $("select[name='DesignationtId']").after('<div class="error-msg text-danger">Please select a department</div>');
    //    } else {
    //        $("select[name='DesignationtId']").next(".error-msg").show();
    //    }
    //    isValid = false;
    //} else {
    //    $("select[name='DesignationtId']").removeClass("is-invalid");
    //    $("select[name='DesignationtId']").next(".error-msg").hide();
    //}
    ////role
    var roleValue = $("select[name='RoleId']").val();
    if (!roleValue || roleValue === "") {
        $("select[name='RoleId']").addClass("is-invalid");
        // Show error message below dropdown
        if ($("select[name='RoleId']").next(".error-msg").length === 0) {
            $("select[name='RoleId']").after('<div class="error-msg text-danger">Please select a department</div>');
        } else {
            $("select[name='RoleId']").next(".error-msg").show();
        }
        isValid = false;
    } else {
        $("select[name='RoleId']").removeClass("is-invalid");
        $("select[name='RoleId']").next(".error-msg").hide();
    }
    ////manager
    //var manValue = $("select[name='ReportingManager']").val();
    //if (!manValue || manValue === "") {
    //    $("select[name='ReportingManager']").addClass("is-invalid");
    //    // Show error message below dropdown
    //    if ($("select[name='ReportingManager']").next(".error-msg").length === 0) {
    //        $("select[name='ReportingManager']").after('<div class="error-msg text-danger">Please select a department</div>');
    //    } else {
    //        $("select[name='ReportingManager']").next(".error-msg").show();
    //    }
    //    isValid = false;
    //} else {
    //    $("select[name='ReportingManager']").removeClass("is-invalid");
    //    $("select[name='ReportingManager']").next(".error-msg").hide();
    //}

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
    var genderValue = $("select[name='Gender']").val();
    if (!genderValue || genderValue === "Select") {
        $("select[name='Gender']").addClass("is-invalid");
        // Show error message below dropdown
        if ($("select[name='Gender']").next(".error-msg").length === 0) {
            $("select[name='Gender']").after('<div class="error-msg text-danger">Please select a valid status</div>');
        } else {
            $("select[name='Gender']").next(".error-msg").show();
        }
        isValid = false;
    } else {
        $("select[name='Gender']").removeClass("is-invalid");
        $("select[name='Gender']").next(".error-msg").hide();
    }

    //about
    //about
    var aboutValue = $("textarea[name='AboutEmployee']").val().trim();
    if (!aboutValue) {
        $("textarea[name='AboutEmployee']").addClass("is-invalid");
        if ($("textarea[name='AboutEmployee']").next(".error-msg").length === 0) {
            $("textarea[name='AboutEmployee']").after('<div class="error-msg text-danger">This field is required</div>');
        } else {
            $("textarea[name='AboutEmployee']").next(".error-msg").show();
        }
        isValid = false;
    } else {
        $("textarea[name='AboutEmployee']").removeClass("is-invalid");
        $("textarea[name='AboutEmployee']").next(".error-msg").hide();
    }

    var profileInput = $("#ProfilePicture")[0]; // Get file input
    if (profileInput.files.length > 0) {
        var file = profileInput.files[0];
        var allowedExtensions = ["jpg", "jpeg", "png", "gif"];
        var fileExtension = file.name.split('.').pop().toLowerCase();
        var allowedTypes = ["image/jpeg", "image/png", "image/gif"];

        if (!allowedExtensions.includes(fileExtension) || !allowedTypes.includes(file.type)) {
            alert("Only image files (JPEG, PNG, GIF) are allowed.");
            return;
        }
    }
    // Stop form submission if validation fails
    if (!isValid) return;

    var obj = new FormData($("#myempform")[0]);

    for (var pair of obj.entries()) {
        console.log(pair[0] + ': ' + pair[1]);
    }

    $.ajax({
        url: '/Employee/AddEmp',
        type: 'POST',
        contentType: false,
        processData: false,
        data: obj,
        dataType: 'json',
        success: function () {
            alert("User Added Successfully");
            $('#exampleModal').modal('hide');
            fetchUser();
            location.reload();
        },
        error: function () {
            alert("Something went wrong");
        }
    });

});


function fetchUser() {
    $.ajax({
        url: '/Employee/FetchEmp',
        type: 'Get',
        contentType: 'application/json; charset=utf8',
        dataType: 'json',
        success: function (result, status, xhr) {
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
                obj += "<td>" + item.userId + "</td>";
                obj += "<td>";
                obj += "<img src='/" + item.profilePicture + "' alt='Profile Picture' width='50' height='50' class='rounded-circle' />";
                obj += " " + item.firstName + " " + item.lastName;
                obj += "</td>";
                obj += "<td>" + item.email + "</td>";
                obj += "<td>" + item.phoneNumber + "</td>";
                obj += "<td>" + item.designation.name + "</td>";
                obj += "<td>" + item.reportingManager + "</td>";
                //obj += "<td>" + (item.department ? item.department.name : '') + "</td>";
                //obj += "<td>" + (item.designation ? item.designation.name : '') + "</td>";
                obj += "<td>" + item.dateOfJoining.split("T")[0] + "</td>";
                obj += "<td>" + statusBadge + "</td>";
                 obj += "<td>" + item.createdBy + "</td>";
                // obj += "<td>" + item.createdAt + "</td>";
                obj += "<td>" + item.modifiedBy + "</td>";
                //obj += "<td>" + item.modifiedAt + "</td>";
                //obj += "<td>" + item.status + "</td>";
                obj += "<td>";
                obj += "<a href='#' class='edit-btn' data-id='" + item.userId + "'><i class='ti ti-edit'></i></a> ";
                obj += "<a href='#' class='delete-btn' data-id='" + item.userId + "'><i class='bi bi-trash'></i></a>";
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
                "pageLength": 5,
                "lengthMenu": [[5, 10, 25, -1], [5, 10, 25, "All"]], // Dropdown for selecting rows per page
                "ordering": true,      // Enable column sorting
                "searching": true,     // Enable search box

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
//        fetchUserDetails(id);
//    });

//    $(".delete-btn").click(function () {
//        var id = $(this).data("id");
//        if (confirm("Are you sure you want to delete this Emp?")) {
//            deleteUser(id);
//        }
//    });
//}
function attachEventHandlers() {
    // Use event delegation for edit buttons
    $('#table').on('click', '.edit-btn', function (e) {
        e.preventDefault(); // Prevent default action (e.g., following a link)
        var id = $(this).data("id");
        fetchUserDetails(id); // Fetch details and show edit modal
    });

    // Use event delegation for delete buttons
    $('#table').on('click', '.delete-btn', function (e) {
        e.preventDefault(); // Prevent default action (e.g., following a link)
        var id = $(this).data("id");
        if (confirm("Are you sure you want to delete this User?")) {
            deleteUser(id);
        }
    });
}


//start with edit


function fetchUserDetails(id) {
    $.ajax({
        url: '/Employee/EditEmp?eid=' + id,
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            if (response.success) {
                var desi = response.data;

                $("#editUserId").val(desi.userId);
                $("#editProfilePicturePreview").attr("src", desi.profilePicture);
                $("#editFirstName").val(desi.firstName);
                $("#editLastName").val(desi.lastName);
                $("#editEmail").val(desi.email);
                $("#editPasswordHash").val(desi.passwordHash);
                $("#editPhoneNumber").val(desi.phoneNumber);

                //$("#editDepartmentDropdown").val(user.departmentId);
                $("#editManagerDropdown").val(desi.reportingManager);
                //$("#editDesigntionDropdown").val(user.designationId);

                if (desi.departmentId) {
                    $("#editDepartmentDropdown").val(desi.departmentId);
                }
                if (desi.designationId) {
                    $("#editDesigntionDropdown").val(desi.designationId);
                }

                $("#editDateOfJoining").val(desi.dateOfJoining);
                $("#editStatus").val(desi.status);
                $("#editDateOfBirth").val(desi.dateOfBirth);
                $("#editGender").val(desi.gender);
                $("#editAddress").val(desi.address);
                $("#editAboutEmployee").val(desi.aboutEmployee);

                // Populate role dropdown
                var roleDropdown = $("#RoleId");
                roleDropdown.empty(); // Clear previous options

                // Find the selected roleName based on RoleId
                var selectedRole = response.users.find(role => role.roleId == desi.roleId);

                // Add the selected role first
                if (selectedRole) {
                    roleDropdown.append(`<option value="${selectedRole.roleId}" selected>${selectedRole.roleName}</option>`);
                }

                // Add remaining roles (excluding the selected one)
                $.each(response.users, function (i, role) {
                    if (role.roleId != desi.roleId) { // Compare roleId instead of userId
                        roleDropdown.append(`<option value="${role.roleId}">${role.roleName}</option>`);
                    }
                });

                


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

// Fetch emp Details for Edit






    //Ganesh
    //$.ajax({
    //    url: '/Employee/FetchEmp',
    //    type: 'GET',
    //    dataType: 'json',
    //    success: function (result) {
    //        var user = result.find(d => d.userId === id);
    //        if (user) {
    //            $("#editUserId").val(user.userId);
    //            $("#editProfilePicturePreview").attr("src", user.profilePicture);
    //            $("#editFirstName").val(user.firstName);
    //            $("#editLastName").val(user.lastName);
    //            $("#editEmail").val(user.email);
    //            $("#editPasswordHash").val(user.passwordHash);
    //            $("#editPhoneNumber").val(user.phoneNumber);
    //            $("#roleDropdown").val(user.roleId);
    //            $("#departmentDropdown").val(user.departmentId);
    //            $("#managerDropdown").val(user.reportingManager);
    //            $("#editDesignation").val(user.designationId);
    //            $("#editDateOfJoining").val(user.dateOfJoining);
    //            $("#editStatus").val(user.status);
    //            $("#editDateOfBirth").val(user.dateOfBirth);
    //            $("#editGender").val(user.gender);
    //            $("#editAddress").val(user.address);
    //            $("#editAboutEmployee").val(user.aboutEmployee);
    //            $("#editModal").modal("show");
    //        }
    //    },
    //    error: function () {
    //        alert("Failed to fetch department details");
    //    }
    //});

//function fetchUserDetails(id) {
//    $.ajax({
//        url: '/Employee/FetchEmp',
//        type: 'GET',
//        dataType: 'json',
//        success: function (response) {
//            if (response.success) {
//                var users = response.users;
//                var roles = response.roles;

//                var user = users.find(d => d.userId === id);
//                if (user) {
//                    $("#editUserId").val(user.userId);
//                    $("#editProfilePicturePreview").attr("src", user.profilePicture);
//                    $("#editFirstName").val(user.firstName);
//                    $("#editLastName").val(user.lastName);
//                    $("#editEmail").val(user.email);
//                    $("#editPasswordHash").val(user.passwordHash);
//                    $("#editPhoneNumber").val(user.phoneNumber);
//                    $("#editManagerDropdown").val(user.reportingManager);
//                    $("#editDateOfJoining").val(user.dateOfJoining);
//                    $("#editStatus").val(user.status);
//                    $("#editDateOfBirth").val(user.dateOfBirth);
//                    $("#editGender").val(user.gender);
//                    $("#editAddress").val(user.address);
//                    $("#editAboutEmployee").val(user.aboutEmployee);

//                    // ✅ Set Department & Designation Dropdowns
//                    if (user.departmentId) {
//                        $("#editDepartmentDropdown").val(user.departmentId);
//                    }
//                    if (user.designationId) {
//                        $("#editDesigntionDropdown").val(user.designationId);
//                    }

//                    // ✅ Populate and Set Role Dropdown
//                    var roleDropdown = $("#editRoleDropdown");
//                    roleDropdown.empty(); // Clear previous options
//                    roleDropdown.append('<option value="">-- Select Role --</option>'); // Default option

//                    $.each(roles, function (i, role) {
//                        var selected = (role.roleId == user.roleId) ? "selected" : "";
//                        roleDropdown.append(`<option value="${role.roleId}" ${selected}>${role.roleName}</option>`);
//                    });

//                    $("#editModal").modal("show");
//                }
//            } else {
//                alert("Failed to fetch user details.");
//            }
//        },
//        error: function () {
//            alert("Error fetching user details.");
//        }
//    });
//}


// Save Edited user 
$("#saveEdit").click(function () {
    //var obj = $("#editForm").serialize();
    var obj = new FormData($("#editForm")[0]);
    $.ajax({
        url: '/Employee/EditUser',
        type: 'POST',
        data: obj,
        processData: false,
        contentType: false,
        success: function () {
            alert("User updated successfully");
            $("#editModal").modal("hide");
            fetchUser();
            location.reload();
        },
        error: function () {
            alert("Failed to update department");
        }
    });
});


// Delete emp
//function deleteUser(id) {
//    $.ajax({
//        url: '/Employee/DeleteUser',
//        type: 'POST',
//        data: { id: id },
//        success: function () {
//            alert("User deleted successfully");
//            fetchUser();
//            location.reload();
//        },
//        error: function () {
//            alert("Failed to delete department");
//        }
//    });
//}

function deleteUser(id) {
    $.ajax({
        url: '/Employee/DeleteUser',
        type: 'POST',
        data: { id: id },
        success: function () {
            alert("User deleted successfully");
            fetchUser();
            location.reload();
        },
        error: function (xhr, status, error) {
            var errorMessage = xhr.responseText ? JSON.parse(xhr.responseText).message : "An unexpected error occurred.";
            alert("Error: " + errorMessage);
        }
    });
}

//start with filters
//$("#opt").on('click', function () {
//    var data = $("#opt").val();
//    $.ajax({
//        url: '/Employee/SortingByAsc?mydata=' + data,
//        type: 'Get',
//        contentType: 'application/json; charset=utf8',
//        dataType: 'json',
//        success: function (result, status, xhr) {
//            console.log(result);
//            var obj = '';
//            $.each(result, function (index, item) {
//                console.log(item);
//                obj += "<tr>";
//                obj += "<td>" + item.userId + "</td>";
//                obj += "<td><img src='/" + item.profilePicture + "' alt='Profile Picture' width='50' height='50'/></td>";
//                obj += "<td>" + item.firstName + "</td>";
//                obj += "<td>" + item.lastName + "</td>";
//                obj += "<td>" + item.email + "</td>";
//                obj += "<td>" + item.passwordHash + "</td>";
//                obj += "<td>" + item.phoneNumber + "</td>";
//                obj += "<td>" + item.role.roleName + "</td>";
//                obj += "<td>" + item.department.name + "</td>";
//                obj += "<td>" + item.designation.name + "</td>";
//                obj += "<td>" + item.reportingManager + "</td>";
//                obj += "<td>" + item.dateOfJoining + "</td>";
//                obj += "<td>" + item.status + "</td>";
//                obj += "<td>" + item.dateOfBirth + "</td>";
//                obj += "<td>" + item.gender + "</td>";
//                obj += "<td>" + item.address + "</td>";
//                obj += "<td>" + item.aboutEmployee + "</td>";

//                obj += "<td>";
//                obj += "<a href='#' class='edit-btn' data-id='" + item.userId + "'><i class='fas fa-edit text-warning'></i></a> ";
//                obj += "<a href='#' class='delete-btn' data-id='" + item.userId + "'><i class='fas fa-trash-alt text-danger'></i></a>";
//                obj += "</td>";
//                obj += "</tr>";
//            });
//            $("#mydata").html(obj);
//        },
//        error: function () {
//            alert("Something went wrong");
//        }
//    })
//});



//$("#opt").on('change', function () {
//    var selectedOption = $("#opt").val(); // Get selected value

//    $.ajax({
//        url: '/Employee/SortingByCriteria',
//        type: 'GET',
//        data: { sortBy: selectedOption },
//        contentType: 'application/json; charset=utf-8',
//        dataType: 'json',
//        success: function (result) {
//            console.log(result); // Debugging

//            var obj = '';

//            // Check if result is empty or contains an error
//            if (!result || result.length === 0 || result.error) {
//                obj = `<tr><td colspan="9" class="text-center text-danger">No records found</td></tr>`;
//            } else {
//                $.each(result, function (index, item) {
//                    let statusBadge = '';

//                    if (item.status === "Active") {
//                        statusBadge = `<span class="badge bg-success">${item.status}</span>`; // Green badge
//                    } else if (item.status === "Inactive") {
//                        statusBadge = `<span class="badge bg-danger">${item.status}</span>`; // Red badge
//                    } else {
//                        statusBadge = `<span class="badge bg-secondary">${item.status}</span>`; // Default gray badge
//                    }

//                    obj += `<tr>
//                        <td>${item.userId}</td>
//                        <td>
//                            <img src='/${item.profilePicture}' alt='Profile Picture' width='50' height='50' class='rounded-circle' />
//                            ${item.firstName} ${item.lastName}
//                        </td>
//                        <td>${item.email}</td>
//                        <td>${item.phoneNumber}</td>
//                        <td>${item.designationName}</td>
//                        <td>${item.reportingManager}</td>
//                        <td>${item.dateOfJoining}</td>
//                        <td>${statusBadge}</td>
//                        <td>
//                            <a href='#' class='edit-btn' data-id='${item.userId}'><i class='fas fa-edit text-warning'></i></a>
//                            <a href='#' class='delete-btn' data-id='${item.userId}'><i class='fas fa-trash-alt text-danger'></i></a>
//                        </td>
//                    </tr>`;
//                });
//            }

//            $("#mydata").html(obj); // Update table
//            attachEventHandlers();  // Reattach event handlers
//        },
//        error: function (xhr, status, error) {
//            console.error("AJAX Error:", status, error);
//            alert("Failed to sort/filter users. Please check the console for details.");
//        }
//    });
//});



$("#opt").on('change', function () {
    var selectedOption = $("#opt").val(); // Get selected sort option
    console.log("Sorting By:", selectedOption); // Debugging

    $.ajax({
        url: '/Employee/SortUser',
        type: 'GET',
        data: { sortBy: selectedOption },
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        success: function (result) {
            console.log("Response:", result); // Debugging response

            var obj = '';

            if (!result || result.length === 0 || result.error) {
                obj = `<tr><td colspan="9" class="text-center text-danger">No records found</td></tr>`;
            } else {
                $.each(result, function (index, item) {
                    let statusBadge = item.status === "Active"
                        ? `<span class="badge bg-success"><i class="ti ti-point-filled me-1"></i>${item.status}</span>`
                        : `<span class="badge bg-danger"><i class="ti ti-point-filled me-1"></i>${item.status}</span>`;

                    obj += `<tr>
                            <td>${item.userId}</td>
                            <td>
                                <img src='/${item.profilePicture}' alt='Profile Picture' width='50' height='50' class='rounded-circle' />
                                ${item.firstName} ${item.lastName}
                            </td>
                            <td>${item.email}</td>
                            <td>${item.phoneNumber}</td>
                            <td>${item.designationName}</td>
                            <td>${item.reportingManager}</td>
                            <td>${item.dateOfJoining}</td>
                            <td>${statusBadge}</td>
                            <td>${item.createdBy}</td>
                            <td>${item.modifiedBy}</td>
                            <td>
                                <a href='#' class='edit-btn' data-id='${item.userId}'><i class='ti ti-edit'></i></a>
                                <a href='#' class='delete-btn' data-id='${item.userId}'><i class='bi bi-trash'></i></a>
                            </td>
                        </tr>`;
                });
            }

            $("#mydata").html(obj); // Update table
            attachEventHandlers();  // Reattach event handlers for edit and delete buttons
        },
        error: function (xhr, status, error) {
            console.error("AJAX Error:", status, error);
            console.log("Response Text:", xhr.responseText);
            alert("Sorting failed. Check the console.");
        }
    });
});



$("#sts").on('change', function () {
    var data = $(this).val(); // Get the selected value from the dropdown
    $.ajax({
        url: '/Employee/SortingUserStatus?status=' + data, // Call the SortingStatus endpoint
        type: 'GET',
        contentType: 'application/json; charset=utf8',
        dataType: 'json',
        success: function (result) {
            console.log(result); // Debugging: Check the response in the console

            var obj = '';

            // Check if result is empty or contains an error
            if (!result || result.length === 0 || result.error) {
                obj = `<tr><td colspan="9" class="text-center text-danger">No records found</td></tr>`;
            } else {
                $.each(result, function (index, item) {
                    let statusBadge = '';

                    if (item.status === "Active") {
                        statusBadge = `<span class="badge bg-success"><i class="ti ti-point-filled me-1"></i>${item.status}</span>`; // Green badge
                    } else if (item.status === "Inactive") {
                        statusBadge = `<span class="badge bg-danger"><i class="ti ti-point-filled me-1"></i>${item.status}</span>`; // Red badge
                    } else {
                        statusBadge = `<span class="badge bg-secondary"><i class="ti ti-point-filled me-1"></i>${item.status}</span>`; // Default gray badge
                    }

                    obj += "<tr>";
                    obj += "<td>" + item.userId + "</td>";
                    obj += "<td>";
                    obj += "<img src='/" + item.profilePicture + "' alt='Profile Picture' width='50' height='50' class='rounded-circle' />";
                    obj += " " + item.firstName + " " + item.lastName;
                    obj += "</td>";
                    obj += "<td>" + item.email + "</td>";
                    obj += "<td>" + item.phoneNumber + "</td>";
                    obj += "<td>" + item.designationName + "</td>";
                    obj += "<td>" + item.reportingManager + "</td>";
                    obj += "<td>" + item.dateOfJoining + "</td>";
                    obj += "<td>" + statusBadge + "</td>";
                    obj += "<td>" + item.createdBy + "</td>";
                    obj += "<td>" + item.modifiedBy + "</td>";
                    obj += "<td>";
                    obj += "<a href='#' class='edit-btn' data-id='" + item.userId + "'><i class='ti ti-edit'></i></a> ";
                    obj += "<a href='#' class='delete-btn' data-id='" + item.userId + "'><i class='bi bi-trash'></i></a>";
                    obj += "</td>";
                    obj += "</tr>";
                });
            }

            $("#mydata").html(obj); // Update table

            attachEventHandlers();  // Reattach event handlers for edit and delete buttons
        },
        error: function () {
            alert("Failed to filter departments by status.");
        }
    });
});


$("#designation").on('change', function () {
    var designationName = $("#designation option:selected").text(); // Get selected designation name

    $.ajax({
        url: '/Employee/SortingUserByDesignation?designationName=' + encodeURIComponent(designationName),
        type: 'GET',
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        success: function (result) {
            console.log(result); // Debugging: Check the response in the console

            var obj = '';

            // Check if result is empty or contains an error
            if (!result || result.length === 0 || result.error) {
                obj = `<tr><td colspan="9" class="text-center text-danger">No records found</td></tr>`;
            } else {
                $.each(result, function (index, item) {
                    let statusBadge = '';

                    if (item.status === "Active") {
                        statusBadge = `<span class="badge bg-success"><i class="ti ti-point-filled me-1"></i>${item.status}</span>`; // Green badge
                    } else if (item.status === "Inactive") {
                        statusBadge = `<span class="badge bg-danger"><i class="ti ti-point-filled me-1"></i>${item.status}</span>`; // Red badge
                    } else {
                        statusBadge = `<span class="badge bg-secondary"><i class="ti ti-point-filled me-1"></i>${item.status}</span>`; // Default gray badge
                    }

                    obj += "<tr>";
                    obj += "<td>" + item.userId + "</td>";
                    obj += "<td>";
                    obj += "<img src='/" + item.profilePicture + "' alt='Profile Picture' width='50' height='50' class='rounded-circle' />";
                    obj += " " + item.firstName + " " + item.lastName;
                    obj += "</td>";
                    obj += "<td>" + item.email + "</td>";
                    obj += "<td>" + item.phoneNumber + "</td>";
                    obj += "<td>" + item.designationName + "</td>";
                    obj += "<td>" + item.reportingManager + "</td>";
                    obj += "<td>" + item.dateOfJoining + "</td>";
                    obj += "<td>" + statusBadge + "</td>";
                    obj += "<td>" + item.createdBy + "</td>";
                    obj += "<td>" + item.modifiedBy + "</td>";
                    obj += "<td>";
                    obj += "<a href='#' class='edit-btn' data-id='" + item.userId + "'><i class='ti ti-edit'></i></a> ";
                    obj += "<a href='#' class='delete-btn' data-id='" + item.userId + "'><i class='bi bi-trash'></i></a>";
                    obj += "</td>";
                    obj += "</tr>";
                });
            }

            $("#mydata").html(obj); // Update table

            attachEventHandlers();  // Reattach event handlers for edit and delete buttons
        },
        error: function (xhr, status, error) {
            console.error("AJAX Error:", status, error);
            alert("Failed to filter designation. Please check the console for details.");
        }
    });
});



$("#startDate, #endDate").on('change', function () {
    var startDate = $("#startDate").val(); // Get selected start date
    var endDate = $("#endDate").val(); // Get selected end date

    $.ajax({
        url: '/Employee/FilterUsersByDate',
        type: 'GET',
        data: {
            startDate: startDate,
            endDate: endDate
        },
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        success: function (result) {
            console.log(result); // Debugging: Check the response in the console

            var obj = '';

            if (!result || result.length === 0 || result.error) {
                obj = `<tr><td colspan="9" class="text-center text-danger">No records found</td></tr>`;
            } else {
                $.each(result, function (index, item) {
                    let statusBadge = item.status === "Active"
                        ? `<span class="badge bg-success"><i class="ti ti-point-filled me-1"></i>${item.status}</span>`
                        : `<span class="badge bg-danger"><i class="ti ti-point-filled me-1"></i>${item.status}</span>`;

                    obj += `<tr>
                        <td>${item.userId}</td>
                        <td>
                            <img src='/${item.profilePicture}' alt='Profile Picture' width='50' height='50' class='rounded-circle' />
                            ${item.firstName} ${item.lastName}
                        </td>
                        <td>${item.email}</td>
                        <td>${item.phoneNumber}</td>
                        <td>${item.designationName}</td>
                        <td>${item.reportingManager}</td>
                        <td>${item.dateOfJoining}</td>
                        <td>${statusBadge}</td>
                                                <td>${item.createdBy}</td>
                                                                        <td>${item.modifiedBy}</td>
                        <td>
                            <a href='#' class='edit-btn' data-id='${item.userId}'><i class='ti ti-edit'></i></a>
                            <a href='#' class='delete-btn' data-id='${item.userId}'><i class='bi bi-trash'></i></a>
                        </td>
                    </tr>`;
                });
            }

            $("#mydata").html(obj); // Update table
            attachEventHandlers(); // Reattach event handlers
        },
        error: function (xhr, status, error) {
            console.error("AJAX Error:", status, error);
            alert("Failed to filter by date. Please check the console for details.");
        }
    });
});


//start with export

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
    doc.save('User.pdf');
});

//$("#exportToExcel").click(function () {
//    var wb = XLSX.utils.book_new();
//    var ws = XLSX.utils.table_to_sheet($('#mydata')[0]);


//    // Add sheet to the workbook
//    XLSX.utils.book_append_sheet(wb, ws, "Departments");

//    // Download the Excel file
//    XLSX.writeFile(wb, 'User.xlsx');
//});
$("#exportToExcel").click(function () {
    // Store the last column elements
    var lastTh = $('#table th:last-child').detach();
    var lastTds = $('#table td:last-child').detach();

    var wb = XLSX.utils.book_new();
    var ws = XLSX.utils.table_to_sheet($('#table')[0]);

    // Add sheet to the workbook
    XLSX.utils.book_append_sheet(wb, ws, "Users");

    // Download the Excel file
    XLSX.writeFile(wb, 'Users.xlsx');

    // Restore the last column
    $('#table thead tr').append(lastTh);
    $('#table tbody tr').each(function (index, row) {
        $(row).append(lastTds[index]);
    });
});