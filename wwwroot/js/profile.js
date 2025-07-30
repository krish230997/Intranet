$(document).ready(function () {
    // Attach event handlers



    //fetch and open edit profile model 
    $("#userId").click(function () {
        var userId = $(this).val();
        fetchUserDetails(userId); // Fetch user details 
    });

    function fetchUserDetails(id) {
        $.ajax({
            url: `/Employee/FetchUserDetails/${id}`,
            type: 'GET',
            dataType: 'json',
            success: function (response) {
                if (response.success) {
                    //const user = response.data;
                    // Populate the modal fields
                    $("#editUserId").val(response.userId);
                    $("#editProfileModal").modal("show");
                    //$("#FirstName").val(response.firstName);
                    //alert('This is alert');
                    //$("#editLastName").val(user.LastName);
                    //$("#editEmail").val(user.Email);
                    //$("#editPhoneNumber").val(user.PhoneNumber);
                    //$("#editAddress").val(user.Address);
                    //$("#editAboutEmployee").val(user.AboutEmployee);
                    //$("#editDateOfBirth").val(user.DateOfBirth);

                    // If profile picture exists, display it as a preview
                    //if (user.ProfilePicture) {
                    //    $("#editProfilePicture").attr("src", user.ProfilePicture);
                    //}

                    // Open the modal

                } else {
                    alert("Failed to fetch Employeeeeeeeeeeeeeeeeeeeeeeee details.");
                }
            },
            error: function () {
                alert("An error occurred while fetching user details.");
            }
        });
    }
    //Save Edit Profile
    $("#saveEditProfile").click(function () {
        // Create FormData object to handle file uploads and form data
        var obj = new FormData($("#editProfileForm")[0]);

        $.ajax({
            url: '/Employee/UpdateProfile',
            type: 'POST',
            data: obj,
            processData: false, // Prevent jQuery from processing the data
            contentType: false, // Prevent jQuery from setting the content type
            success: function (response) {
                if (response.success) {
                    
                    $("#editProfileModal").modal("hide");
                    
                    // Update the profile section with new data
                    const updatedUser = response.data;
                    $(".profile-info h2").text(`${updatedUser.FirstName} ${updatedUser.LastName}`);
                    $(".profile-info p:nth-child(2)").text(`${updatedUser.Designation} at ${updatedUser.Department}`);
                    $(".profile-info p:nth-child(3)").text(updatedUser.Role);
                    $(".detail:contains('Email')").html(`<strong>Email:</strong> ${updatedUser.Email}`);
                    $(".detail:contains('Phone Number')").html(`<strong>Phone Number:</strong> ${updatedUser.PhoneNumber}`);
                    $(".detail:contains('Date of Birth')").html(`<strong>Date of Birth:</strong> ${updatedUser.DateOfBirth}`);
                    $(".detail:contains('Address')").html(`<strong>Address:</strong> ${updatedUser.Address}`);
                    $(".profile-picture img").attr("src", updatedUser.ProfilePicture || "/default-profile.png");
                    $(".detail:contains('About')").text(updatedUser.AboutEmployee);

                    alert("Profile updated successfully!");
                    window.history.replaceState({}, document.title, "/Employee/UserProfile");
                    location.reload();
                } else {
                    alert("Failed to update user.");
                }

                
            },
            error: function () {
                alert("An error occurred while updating the user.");
            }
        });
    });


    //Add Family Detail
    $("#addFamilyDetails").click(function () {
        // console.log("Modal Trigger Clicked");
        $("#addFamilyDetailsModal").modal("show");
    });

    //Save Family Detail
    $("#saveFamilyDetails").click(function () {

        var isValid = true; // Validation flag
        // Validate required input fields and textareas
        $("#addFamilyDetailsForm input[required]").each(function () {
            if ($(this).val().trim() === "") {
                $(this).addClass("is-invalid");
                $(this).next(".error-msg").text("This field is required").show();
                isValid = false;
            } else {
                $(this).removeClass("is-invalid");
                $(this).next(".error-msg").hide();
            }
        });
        // Stop form submission if validation fails
        if (!isValid) return;

        var obj = $("#addFamilyDetailsForm").serialize();

        $.ajax({
            url: '/Employee/AddFamilyDetail',
            type: 'POST',
            contentType: 'application/x-www-form-urlencoded; charset=utf8',
            data: obj,
            dataType: 'json',
            success: function () {
                alert("Family Detail Added Successfully");
                $('#addFamilyDetailsModal').modal('hide');
                fetchUserDetails(userId);
                location.reload();
            },
            error: function () {
                alert("Something went wrong");
            }
        });
    });


    //Open Edit Family Detail Model
    $("#editFamilyDetails").click(function () {
        //var familyId = $(this).data("familyid");
        //alert("welcome to edit family");
        //$("#editFamilyDetailsModal").modal("show");
        var familyId = $(this).val();
        console.log("Fetching details for Family ID:", familyId);
        FamilyDetailsEdit(familyId); // Fetch family details
    });

    //Edit Family detail
    function FamilyDetailsEdit(id) {
        $.ajax({
            url: '/Employee/FindFamilyDetails?id=' + id,
            type: 'GET',
            contentType: 'application/json; charset=utf8',
            dataType: 'json',
            success: function (response) {
                $("#editFamilyDetailsModal").modal("show");
                $("#FamilyDetailId").val(response.familyDetailId);
                $("#editName").val(response.name);
                $("#editRelation").val(response.relation);
                $("#UserId").val(response.userId);
                $("#editDateOfBirth").val(response.dateOfBirth.split('T')[0]); // Format date
                $("#editPhone").val(response.phone);
            },
            error: function () {
                alert("Wrong");
            }
        });
    }

    //Save Edit family detail
    $("#SaveFamilyDetails").click(function () {
        var obj = $("#editFamilyDetailsForm").serialize();
        $.ajax({
            url: '/Employee/UpdateFamilyDetails',
            type: 'Post',
            dataType: 'json',
            contentType: 'Application/x-www-form-urlencoded;charset=utf-8;',
            data: obj,
            success: function () {
                alert("Successfully Done");

                $("#editFamilyDetailsModal").modal('hide');
                location.reload();

            },
            error: function () {
                alert("Error");
            }
        });
    });


    //Add Family Detail
    $("#addBankDetails").click(function () {
        // console.log("Modal Trigger Clicked");
        $("#addBankDetailsModal").modal("show");
    });

    //Save Family Detail
    $("#saveBankDetails").click(function () {
        var isValid = true; // Validation flag
        // Validate required input fields and textareas
        $("#addBankDetailsForm input[required]").each(function () {
            if ($(this).val().trim() === "") {
                $(this).addClass("is-invalid");
                $(this).next(".error-msg").text("This field is required").show();
                isValid = false;
            } else {
                $(this).removeClass("is-invalid");
                $(this).next(".error-msg").hide();
            }
        });
        // Stop form submission if validation fails
        if (!isValid) return;
        var obj = $("#addBankDetailsForm").serialize();

        $.ajax({
            url: '/Employee/AddBankDetail',
            type: 'POST',
            contentType: 'application/x-www-form-urlencoded; charset=utf8',
            data: obj,
            dataType: 'json',
            success: function () {
                alert("Bank Detail Added Successfully");
                $('#addFamilyDetailsModal').modal('hide');
                fetchUserDetails(userId);
                location.reload();
            },
            error: function () {
                alert("Something went wrong");
            }
        });
    });


    //edit bank details start
    $("#editBankDetails").click(function () {
        var bankId = $(this).val();
        console.log("Fetching details for Family ID:", bankId);
        BankDetailsEdit(bankId); // Fetch family details
    });

    //Edit bank detail
    function BankDetailsEdit(id) {
        $.ajax({
            url: '/Employee/FindBankDetails?id=' + id,
            type: 'GET',
            contentType: 'application/json; charset=utf8',
            dataType: 'json',
            success: function (response) {
                console.log("Response received:", response);
                $("#editbankDetailsModal").modal("show");
                $("#editBankDetailId").val(response.bankDetailId);
                $("#editBankName").val(response.bankName);
                $("#editAccountNumber").val(response.accountNumber);
                $("#editCode").val(response.ifscCode);
                $("#editUserId").val(response.userId);
                $("#editBranchName").val(response.branchName);
            },
            error: function () {
                alert("Wrong");
            }
        });
    }

    //Save Edit family detail
    $("#SaveBankDetails").click(function () {
        var obj = $("#editBankDetailsForm").serialize();
        $.ajax({
            url: '/Employee/UpdateBankDetails',
            type: 'Post',
            dataType: 'json',
            contentType: 'Application/x-www-form-urlencoded;charset=utf-8;',
            data: obj,
            success: function () {
                alert("Successfully Updated Bank Details");
                $("#editBankDetailsModal").modal('hide');
                location.reload();
            },
            error: function () {
                alert("Error");
            }
        });
    });

    //Add Education Detail
    $("#addEducationDetails").click(function () {
        // console.log("Modal Trigger Clicked");
        $("#addEducationDetailsModal").modal("show");
    });

    //Save Education Detail
    $("#saveEducationDetails").click(function () {
        var isValid = true; // Validation flag
        // Validate required input fields and textareas
        $("#addEducationDetailsForm input[required]").each(function () {
            if ($(this).val().trim() === "") {
                $(this).addClass("is-invalid");
                $(this).next(".error-msg").text("This field is required").show();
                isValid = false;
            } else {
                $(this).removeClass("is-invalid");
                $(this).next(".error-msg").hide();
            }
        });
        // Stop form submission if validation fails
        if (!isValid) return;
        var obj = $("#addEducationDetailsForm").serialize();

        $.ajax({
            url: '/Employee/AddEducationDetail',
            type: 'POST',
            contentType: 'application/x-www-form-urlencoded; charset=utf8',
            data: obj,
            dataType: 'json',
            success: function () {
                alert("Education Detail Added Successfully");
                $('#addEducationDetailsModal').modal('hide');
                fetchUserDetails(userId);
                location.reload();
            },
            error: function () {
                alert("Something went wrong");
            }
        });
    });

    //edit bank details start
    $("#editEducationDetails").click(function () {
        var eduId = $(this).val();
        console.log("Fetching details for Education ID:", eduId);
        EduDetailsEdit(eduId); // Fetch family details
    });

    //Edit bank detail
    function EduDetailsEdit(id) {
        $.ajax({
            url: '/Employee/FindEducationDetails?id=' + id,
            type: 'GET',
            //contentType: 'application/json; charset=utf8',
            dataType: 'json',
            success: function (response) {
                console.log("Response received:", response);
                // alert('hello');
                $("#editEducationDetailsId").val(response.educationDetailsId);
                $("#editEducationType").val(response.educationType);
                $("#editUniversityName").val(response.universityName);
                $("#editstartdate").val(response.startdate);
                $("#editUserId").val(response.userId);
                $("#editenddate").val(response.enddate);
                $("#editeducationDetailsModal").modal("show");
            },
            error: function () {
                alert("Wrong");
            }
        });
    }

    //Save Edit edu detail
    $("#saveeditEducationDetails").click(function () {
        var obj = $("#editEducationDetailsForm").serialize();
        $.ajax({
            url: '/Employee/UpdateEducationDetails',
            type: 'Post',
            dataType: 'json',
            contentType: 'Application/x-www-form-urlencoded;charset=utf-8;',
            data: obj,
            success: function () {
                alert("Successfully Updated Education Details");
                $("#editeducationDetailsModal").modal('hide');
                location.reload();
            },
            error: function () {
                alert("Error");
            }
        });
    });

    //Add Experiancce Detail
    $("#addExperienceDetails").click(function () {
        // console.log("Modal Trigger Clicked");
        $("#addExperienceDetailsModal").modal("show");
    });

    //Save Experiance Detail
    $("#saveExperienceDetails").click(function () {
        var isValid = true; // Validation flag
        // Validate required input fields and textareas
        $("#addExperienceDetailsForm input[required]").each(function () {
            if ($(this).val().trim() === "") {
                $(this).addClass("is-invalid");
                $(this).next(".error-msg").text("This field is required").show();
                isValid = false;
            } else {
                $(this).removeClass("is-invalid");
                $(this).next(".error-msg").hide();
            }
        });
        // Stop form submission if validation fails
        if (!isValid) return;
        var obj = $("#addExperienceDetailsForm").serialize();

        $.ajax({
            url: '/Employee/AddExperianceDetail',
            type: 'POST',
            contentType: 'application/x-www-form-urlencoded; charset=utf8',
            data: obj,
            dataType: 'json',
            success: function () {
                alert("Education Detail Added Successfully");
                $('#addExperienceDetailsModal').modal('hide');
                fetchUserDetails(userId);
                location.reload();
            },
            error: function () {
                alert("Something went wrong");
            }
        });
    });

    //edit edu details start
    $("#editExperienceDetails").click(function () {
        var exId = $(this).val();
        console.log("Fetching details for experiance ID:", exId);
        ExDetailsEdit(exId); // Fetch family details
    });
    function ExDetailsEdit(id) {
        $.ajax({
            url: '/Employee/FindExperienceDetails?id=' + id,
            type: 'GET',
            contentType: 'application/json; charset=utf8',
            dataType: 'json',
            success: function (response) {
                console.log("Response received:", response);

                $("#editExperienceId").val(response.experienceId);
                $("#editCompanyName").val(response.companyName);
                $("#editDesignationName").val(response.designationName);
                $("#editFromDate").val(response.fromDate);
                $("#editUserId").val(response.userId);
                $("#editToDate").val(response.toDate);
                $("#editexperienceDetailsModal").modal("show");
            },
            error: function () {
                alert("Wrong");
            }
        });
    }

    //Save Edit family detail
    $("#saveeditExperienceDetails").click(function () {
        var obj = $("#editExperienceDetailsForm").serialize();
        $.ajax({
            url: '/Employee/UpdateExperienceDetails',
            type: 'Post',
            dataType: 'json',
            contentType: 'Application/x-www-form-urlencoded;charset=utf-8;',
            data: obj,
            success: function () {
                alert("Successfully Updated Bank Details");
                $("#editexperienceDetailsModal").modal('hide');
                location.reload();
            },
            error: function () {
                alert("Error");
            }
        });
    });

});