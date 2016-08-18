g.ChangeManager = new function()
{
    var self = this;

    /*var Undo;
    if (typeof exports !== 'undefined')
        Undo = exports;
    else
        Undo = window.Undo = {};*/

    self.Stack = V.CreateClass(null,
    {
        constructor: function()
        {
            this.commands = [];
            this.stackPosition = -1;
            //this.savePosition = -1;
        },
        execute: function(command)
        {
            this._clearRedo();
            command.execute();
            this.commands.push(command);
            this.stackPosition++;
            this.changed();
        },
        undo: function()
        {
            this.commands[this.stackPosition].undo();
            this.stackPosition--;
            this.changed();
        },
        canUndo: function()
        {
            return this.stackPosition >= 0;
        },
        redo: function()
        {
            this.stackPosition++;
            this.commands[this.stackPosition].redo();
            this.changed();
        },
        canRedo: function() { return this.stackPosition < this.commands.length - 1; },
        /*save: function()
        {
            this.savePosition = this.stackPosition;
            this.changed();
        },
        dirty: function() { return this.stackPosition != this.savePosition; },*/
        _clearRedo: function()
        {
            //this.commands = this.commands.slice(0, this.stackPosition + 1); // todo; there's probably a more efficient way to do this
            this.commands.splice(this.stackPosition + 1, this.commands.length - (this.stackPosition + 1));
        },
        changed: function () { /*do nothing; override*/ },
        // custom
        getCurrentChange: function() { return this.commands[this.stackPosition]; },
        //updateCurrentChange: function(change) { this.commands[this.stackPosition] = change; }
        //updateCurrentChange: function(changeVars) { extend(this.commands[this.stackPosition], changeVars); }
        clear: function()
        {
            this.stackPosition = -1;
            this.commands = [];
        }
    });

    self.Command = V.CreateClass(null,
    {
        constructor: function(name) { this.name = name; },
        execute: function() { throw new Error("Override me!"); },
        undo: function() { throw new Error("Override me!"); },
        redo: function() { this.execute(); }
    });
};