window.renderApexChart = (barData, barCategories, radialData, radialLabels) => {
    setTimeout(() => {
        if (typeof ApexCharts !== "undefined") {
            console.log("✅ ApexCharts está cargado correctamente");

            // 🔹 Gráfico radial (donut-chart) con nuevos colores
            var radialOptions = {
                series: radialData,
                chart: {
                    height: 350,
                    type: 'radialBar',
                    animations: {
                        enabled: true,
                        easing: 'easeOutCubic',
                        speed: 800
                    }
                },
                plotOptions: {
                    radialBar: {
                        startAngle: -135,
                        endAngle: 225,
                        hollow: {
                            size: '65%',
                            background: 'transparent',
                            image: undefined,
                            imageOffsetX: 0,
                            imageOffsetY: 0,
                            position: 'front',
                        },
                        track: {
                            show: true,
                            startAngle: undefined,
                            endAngle: undefined,
                            background: '#f8f9fa',
                            strokeWidth: '97%',
                            opacity: 1,
                            margin: 5,
                            dropShadow: {
                                enabled: false,
                                top: 0,
                                left: 0,
                                blur: 3,
                                opacity: 0.5
                            }
                        },
                        dataLabels: {
                            name: {
                                fontSize: '16px',
                                color: '#6c757d',
                                fontWeight: 600,
                                offsetY: -10
                            },
                            value: {
                                fontSize: '36px',
                                color: '#343a40',
                                fontWeight: 700,
                                formatter: function (val) {
                                    return val + '%';
                                },
                                offsetY: 5
                            },
                            total: {
                                show: false
                            }
                        }
                    }
                },
                fill: {
                    type: 'gradient',
                    gradient: {
                        shade: 'dark',
                        type: 'horizontal',
                        shadeIntensity: 0.5,
                        gradientToColors: ['#20c997', '#0d6efd'], // Verde a Azul
                        inverseColors: false,
                        opacityFrom: 1,
                        opacityTo: 1,
                        stops: [0, 100]
                    }
                },
                stroke: {
                    lineCap: 'round',
                    dashArray: 0
                },
                colors: ['#681E5B'], // Color base púrpura
                labels: radialLabels,
                responsive: [{
                    breakpoint: 480,
                    options: {
                        chart: {
                            height: 300
                        },
                        plotOptions: {
                            radialBar: {
                                hollow: {
                                    size: '60%'
                                }
                            }
                        }
                    }
                }]
            };

            const radialElement = document.querySelector("#donut-chart");
            if (radialElement) {
                // Destruir gráfico existente si hay uno
                if (window.radialChart) {
                    try {
                        window.radialChart.destroy();
                    } catch (e) {
                        console.log("Error al limpiar gráfico anterior:", e);
                    }
                }

                // Crear nuevo gráfico
                window.radialChart = new ApexCharts(radialElement, radialOptions);
                window.radialChart.render()
                    .then(() => console.log("Gráfico radial renderizado con éxito"))
                    .catch(err => console.error("Error al renderizar gráfico:", err));
            }
        } else {
            console.error("❌ ApexCharts no se cargó correctamente");
        }
    }, 100); // Pequeño retardo para asegurar que el DOM esté listo
};