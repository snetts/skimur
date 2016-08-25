var path = require('path');
var ExtractTextPlugin = require("extract-text-webpack-plugin");

module.exports = {
  entry: [
    'bootstrap-loader',
    './client/web/client'
  ],
  module: {
    loaders: [
      { test: /\.jsx?$/, loaders: ['babel-loader'] },
      { test: /\.css$/, loader: 'css/locals?module' },
      { test: /\.scss$/, loader: 'css/locals?module!sass' },
      { test: /\.(woff2?|ttf|eot|svg)(\?v=[0-9]\.[0-9]\.[0-9])?$/, loader: 'file' },
      { test: /\.(jpeg|jpeg|gif|png|tiff)(\?v=[0-9]\.[0-9]\.[0-9])?$/, loader: 'file' }
    ]
  },
  output: {
    path: path.join(__dirname, 'server', 'src', 'Skimur.Web', 'wwwroot', 'dist'),
    filename: 'client.js',
    libraryTarget: 'this',
    publicPath: '/dist/'
  },
  plugins: [
     new ExtractTextPlugin("styles.css")
  ],
  resolve: {
    extensions: ['', '.js', '.jsx'],
    modulesDirectories: [
      path.join(__dirname, 'node_modules'),
      path.join(__dirname, 'client', 'common'),
      path.join(__dirname, 'client', 'web')
    ]
  }
};
