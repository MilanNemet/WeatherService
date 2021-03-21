let i = 0;

let dates;

let maxValues;
let progMaxValues;

let minValues;
let progMinValues;

dates = Array.from({ length: 10 }, () => 'Date-' + (++i));

maxValues = Array.from({ length: 10 }, () => Math.floor(Math.random() * 30 + 10));
progMaxValues = maxValues.map(x => x - 3 + Math.random() * 6);

minValues = Array.from({ length: 10 }, () => Math.floor(Math.random() * -30 + 5));
progMinValues = minValues.map(x => x + 3 - Math.random() * 6);

for (let i = 0; i < 2; i++) {
    progMaxValues[i] = null;
    progMinValues[i] = null;
}