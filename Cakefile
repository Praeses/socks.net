fs           = require 'fs'
path         = require 'path'
{exec}       = require 'child_process'


task 'build', 'Build project from tools/*.coffee to tools/*.js', ->
  console.log "Building ...."
  exec 'coffee --compile --output ./tools coffee', (err, stdout, stderr) ->
    throw err if err
    console.log stdout + stderr
  console.log "Built"

