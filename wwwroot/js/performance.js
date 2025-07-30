document.addEventListener("DOMContentLoaded", function () {
    let employeeId = 42; // Replace with dynamic employee ID if needed
    let selectedYear = 2024; // Default year

    function fetchPerformanceData(year) {
        fetch(`/Auth/GetPerformanceData?employeeId=${employeeId}&year=${year}`)
            .then(response => response.json())
            .then(data => {
                if (data) {
                    updatePerformanceData(data);
                    renderPerformanceChart(data);
                }
            })
            .catch(error => console.error("Error fetching performance data:", error));
    }

    function updatePerformanceData(data) {
        document.getElementById("performancePercentage").textContent = data.performancePercentage + "%";
        document.getElementById("performanceChange").textContent = data.performanceChange + "%";
    }

    function renderPerformanceChart(data) {
        const ctx = document.getElementById("performanceChart").getContext("2d");

        if (window.performanceChart) {
            window.performanceChart.destroy(); // Destroy previous chart instance
        }

        window.performanceChart = new Chart(ctx, {
            type: "line",
            data: {
                labels: [
                    "Customer Exp", "Marketing", "Management", "Administration",
                    "Presentation", "Quality", "Efficiency", "Integrity",
                    "Professionalism", "Team Work", "Critical Thinking",
                    "Conflict Mgmt", "Attendance", "Meeting Deadlines"
                ],
                datasets: [{
                    label: "Performance Score",
                    data: Object.values(data.scores).map(Number),
                    borderColor: "green",
                    backgroundColor: "rgba(0, 255, 0, 0.2)",
                    fill: true
                }]
            },
            options: {
                responsive: true,
                scales: {
                    y: {
                        beginAtZero: true,
                        max: 100
                    }
                }
            }
        });
    }

    // Handle Year Selection
    document.querySelectorAll(".year-option").forEach(item => {
        item.addEventListener("click", function () {
            selectedYear = this.textContent.trim();
            document.getElementById("selectedYear").textContent = selectedYear;
            fetchPerformanceData(selectedYear);
        });
    });

    // Initial Data Fetch
    fetchPerformanceData(selectedYear);
});
