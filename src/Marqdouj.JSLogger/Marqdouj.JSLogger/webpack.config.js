const path = require('path');

module.exports = {
    entry: './scripts/jsLogger.ts',
    module: {
        rules: [
            // all files with a `.ts`, `.cts`, `.mts` or `.tsx` extension will be handled by `ts-loader`
            {
                test: /\.([cm]?ts|tsx)$/,
                loader: "ts-loader",
                exclude: /node_modules/,
            }
        ],
    },
    resolve: {
        extensions: ['.tsx', '.ts', '.js'],
    },
    output: {
        filename: 'jsLogger-bundle.js',
        path: path.resolve(__dirname, 'wwwroot/js'),
        library: {
            name: 'MarqdoujJsl',
            type: 'var',
        }
    }
};