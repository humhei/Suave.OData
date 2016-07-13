'use strict';

var dbm;
var type;
var seed;

/**
  * We receive the dbmigrate dependency from dbmigrate initially.
  * This enables us to not have to rely on NODE_PATH.
  */
exports.setup = function(options, seedLink) {
  dbm = options.dbmigrate;
  type = dbm.dataType;
  seed = seedLink;
};

exports.up = function(db, callback) {
  db.createTable('people', {
    id: { type: 'serial', primaryKey: true },
    firstName: 'string',
    lastName: 'string',
    age: 'int',
    email: 'string'
  }, callback);
};

exports.down = function(db, callback) {
  db.dropTable('people', callback);
};
