function MenuActive(menuOpt, contName, actName) {

    if(menuOpt == "root")
    {
        $('a').filter(function() {
                
        // Controller : 3 , Action : 4
        return this.href.toString().split("/")[3] == contName 
            && this.href.toString().split("/")[4] == actName;
                
        // Linkself : 1
        }).parent().addClass('active');
    }
    else
    {
        $('a').filter(function() {
                
        // Controller : 3 , Action : 4
        return this.href.toString().split("/")[3] == contName 
            && this.href.toString().split("/")[4] == actName;
                
        // Linkself : 1
        }).parent().addClass('active').parent().parent().addClass('active');
    }
}
