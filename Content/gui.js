var ctxMax = document.getElementById('maxChart').getContext('2d');
var maxChart = new Chart(ctxMax, {
    type: 'line',
    data: {
        labels: dates,
        datasets: [{
            label: 'Napi maximumok',
            data: maxValues,
            // backgroundColor: ['white'],
            borderColor: ['red'],
            borderWidth: 1
        },
        {
            label: 'Prognosztizált maximumok',
            data: progMaxValues,
            // backgroundColor: ['white'],
            borderColor: ['orange'],
            borderWidth: 1
        }]
    },
    options: {
        scales: {
            yAxes: [{
                ticks: {
                    beginAtZero: true
                }
            }]
        }
    }
});

var ctxMin = document.getElementById('minChart').getContext('2d');
var minChart = new Chart(ctxMin, {
    type: 'line',
    data: {
        labels: dates,
        datasets: [{
            label: 'Napi minimumok',
            data: minValues,
            // backgroundColor: ['white'],
            borderColor: ['blue'],
            borderWidth: 1
        },
        {
            label: 'Prognosztizált minimumok',
            data: progMinValues,
            // backgroundColor: ['white'],
            borderColor: ['cyan'],
            borderWidth: 1
        }]
    },
    options: {
        scales: {
            yAxes: [{
                ticks: {
                    beginAtZero: true
                }
            }]
        }
    }
});