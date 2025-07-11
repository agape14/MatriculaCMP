//window.renderApexChart = () => {
//    setTimeout(() => {
//        if (typeof ApexCharts !== "undefined") {
//            console.log("✅ ApexCharts está cargado correctamente");

//            //var options = {
//            //    chart: {
//            //        type: 'bar',
//            //        height: 350
//            //    },
//            //    series: [{
//            //        name: 'Ventas',
//            //        data: [10, 20, 30, 40]
//            //    }],
//            //    xaxis: {
//            //        categories: ['Ene', 'Feb', 'Mar', 'Abr']
//            //    }
//            //};

//            //const chartElement = document.querySelector("#chart");
//            //if (!chartElement) {
//            //    console.error("❌ No se encontró el elemento #chart.");
//            //    return;
//            //}

//            //var chart = new ApexCharts(chartElement, options);
//            //chart.render();

//            var options = {
//                series: [42, 26, 15],
//                chart: {
//                    height: 230,
//                    type: 'donut',
//                },
//                labels: ["Product A", "Product B", "Product C"],
//                plotOptions: {
//                    pie: {
//                        donut: {
//                            size: '75%'
//                        }
//                    }
//                },
//                dataLabels: {
//                    enabled: false
//                },
//                legend: {
//                    show: false,
//                },
//                colors: ['#5664d2', '#1cbb8c', '#eeb902'],

//            };

//            var chart = new ApexCharts(document.querySelector("#donut-chart"), options);
//            chart.render();
//        } else {
//            console.error("❌ ApexCharts no se cargó correctamente.");
//        }
//    }, 0); // retrasa a la próxima iteración del event loop
//};



window.renderApexChart = (barData, barCategories, donutData, donutLabels) => {
    setTimeout(() => {
        if (typeof ApexCharts !== "undefined") {
            console.log("✅ ApexCharts está cargado correctamente");

            // 🔹 Gráfico de barras
            const barOptions = {
                chart: {
                    type: 'bar',
                    height: 350
                },
                series: [{
                    name: 'Ventas',
                    data: barData
                }],
                xaxis: {
                    categories: barCategories
                }
            };

            const barElement = document.querySelector("#chart");
            if (barElement) {
                const barChart = new ApexCharts(barElement, barOptions);
                barChart.render();
            } else {
                console.error("❌ No se encontró el elemento #chart.");
            }

            // 🔹 Gráfico de dona
            const donutOptions = {
                series: donutData,
                chart: {
                    height: 230,
                    type: 'donut',
                },
                labels: donutLabels,
                plotOptions: {
                    pie: {
                        donut: {
                            size: '75%'
                        }
                    }
                },
                dataLabels: {
                    enabled: false
                },
                legend: {
                    show: false
                },
                colors: ['#5664d2', '#1cbb8c', '#eeb902']
            };

            const donutElement = document.querySelector("#donut-chart");
            if (donutElement) {
                const donutChart = new ApexCharts(donutElement, donutOptions);
                donutChart.render();
            } else {
                console.error("❌ No se encontró el elemento #donut-chart.");
            }
        } else {
            console.error("❌ ApexCharts no se cargó correctamente.");
        }
    }, 0);
};
