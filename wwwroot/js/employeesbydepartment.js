document.addEventListener("DOMContentLoaded", function () {
    fetch('/Auth/EmpByDeptGraphData')
        .then(response => response.json())
        .then(data => {
            const ctx = document.getElementById('empDeptChart').getContext('2d');
            const labels = data.map(item => item.departmentName);
            const counts = data.map(item => item.employeeCount);

            new Chart(ctx, {
                type: 'bar',
                data: {
                    labels: labels,
                    datasets: [{
                        label: 'EmployeeCount',
                        data: counts,
                        backgroundColor: 'rgba(255,111, 40, 0.85)',
                        borderColor: 'rgba(255, 111, 40, 1)',
                        borderWidth: 2,
                        borderRadius: 15,
                        borderSkipped: 'none', // Apply rounding to both ends
                    }]
                },
                options: {
                    responsive: true,
                    plugins: {
                        legend: {
                            display: false
                        }
                    },
                    scales: {
                        x: {
                            beginAtZero: true,
                            grid: {
                                color: 'rgba(0, 0, 0, 0.1)', // Grid line color
                                borderDash: [5, 5], // Dashed lines: [dash length, gap length]
                            },
                        },
                        y: {
                            beginAtZero: true,
                            ticks: {
                                autoSkip: false  // Avoids skipping labels if there are many departments
                            },
                            grid: {
                                color: 'rgba(0, 0, 0, 0.1)', // Grid line color
                                borderDash: [5, 5], // Dashed lines: [dash length, gap length]
                            },
                        }
                    },
                    indexAxis: 'y', // This is crucial for creating a horizontal bar chart
                }
            });
        })
        .catch(error => console.error('Error fetching data: ', error));
});