

//////////////////Choosing the communication///////////////////////////////////
/////////////////and the atom installed////////////////////////////////////////

ind = x_choose(["RS-232" ;"USB"; "Ethernet" ;"Wireless"],["Please select the ... 
    type of communication interface: ";"Just double-click on its name. "],"Cancel");
if ind==0 then
    msg=_("ERORR: No types of communication interfaces has been chosen. ");
    messagebox(msg, "ERROR", "error");
    error(msg);
    return;
elseif ind==2
    if (getos() == "Windows") then
        if ~(atomsIsInstalled('serial')) then
            msg=_("ERROR: A serial communication toolbox must be installed.");
            messagebox(msg, "Error", "error");
            error(msg);
            return;
        else
            flag=1;
        end    
    elseif  (getos() == "Linux") then
        if ~(atomsIsInstalled('serialport')) & ~(atomsIsInstalled('serial')) then
            msg=_("ERROR: A serial communication toolbox must be installed.");
            messagebox(msg, "Error", "error");
            error(msg);
            return;
        elseif (atomsIsInstalled('serialport')) & (atomsIsInstalled('serial')) then
            stoolbx = x_choose(['serialport';'serial' ],"Which serial ...
            commiunication toolbox you prefer to use? "," Cancel ")
            if  stoolbx==1 then
                flag=2;
            elseif stoolbx==2 then
                flag=3;
            else
                msg=_("ERROR: No serial toolbox has been chosen. ");
                messagebox(msg, "Error", "error");
                error(msg);
                return;
            end
        elseif (atomsIsInstalled('serialport')) then
            flag=2;
        elseif  (atomsIsInstalled('serial')) then 
            flag=3;     
        end
    else
        msg=_(["WARNING: This program has been tested and works under Gnu/Linux ...
        and Windows."; "On other platforms you may need modify this script. "])
        messagebox(msg, "WARNING", "warning");
        warning(msg);
        return;
    end
else 
    error("Not possible yet.");
    return;
end

///////////////////////////////////////////////////////////////////////////////
///////////////////////Get OS type and serial command//////////////////////////

if (getos() == "Linux") then
    [rep,stat,stderr]=unix_g("ls /dev/ttyACM*");
    if stderr ~= emptystr() then
        msg=_(["No USB device found. ";"Check your USB connection or try ...
            another port. "])
        messagebox(msg, "ERROR", "error");
        error(msg);
        return;
    end
    ind = x_choose(rep,["Please specify which USB port you wanna use for ...
        communication. ";"Just double-click on its name. "],"Cancel");  
    if ind==0 then
        msg=_("ERORR: No serial port has been chosen. ");
        messagebox(msg, "ERROR", "error");
        error(msg);
        return;
    end
    port_name = rep(ind);
end
if (getos() == "Windows") then
    port_name=evstr(x_dialog('Please enter COM port number: ','4'))
    if port_name==[] then
        msg=_("ERORR: No serial port has been chosen. ");
        messagebox(msg, "ERROR", "error");
        error(msg);
        return;
    end
end

///////////////////////////////////////////////////////////////////////////////

global serial_port
if flag==2 then
    serial_port = serialopen(port_name, 9600, 'N', 8, 1);
    while serial_port == -1
        btn=messagebox(["Please check your USB connection, and then click on ...
        Try again. "; "To choose another port click on Change. "], "Error", ...
        "error", [" Try again " " Change "], "modal");
        if ~btn==1 then
            [rep,stat,stderr]=unix_g("ls /dev/ttyACM*");
            ind = x_choose(rep,["Please specify which USB port you wanna use...
            for communication. ";"Just double-click on its name. "],"Cancel");
        if ind==0 then
            msg=_("ERORR: No serial port has been chosen. ");
            messagebox(msg, "ERROR", "error");
            error(msg);
            return;
        end
        port_name = rep(ind);    
        end
        serial_port = serialopen(port_name, 9600, 'N', 8, 1);
    end
elseif flag==1 | flag==3
    serial_port=openserial(port_name,"9600,n,8,1");
    //error(999)
else
    msg=_("ERROR: Could not specify which serial toolbox to use. ");
    messagebox(msg, "Error", "error");
    error(msg);
    return;
end

///////////////////////////////////////////////////////////////////////////////
///////////////////////////Main Windows////////////////////////////////////////

f=figure("dockable","off");
f.resize = "off";
f.menubar_visible = "off";
f.toolbar_visible = "off";
f.figure_name = "Real-time Light Scattering Measurement";
f.tag = "mainWindow";

f.figure_position = [0 0];
f.figure_size = [1200 700];
f.background = color(246,165,1)

// global serial_port;
// serial_port = openserial(4,"9600,n,8,1");

function start_button()
    global Acquisition
    global A
    Acquisition = %t;
    values=[];
    A = [];
    i = 0;
    value=ascii(0);
    motorForward();
    while Acquisition;
            while(value~=ascii(13)) then
                if  flag == 2 then
                    value=serialread(serial_port,1);
                        elseif flag == 1 | flag == 3 then
                    value=readserial(serial_port,1);
                end
                values=values+value;
                v=strsubst(values,string(ascii(10)),'')
                v=strsubst(v,string(ascii(13)),'')
                data = evstr(v)
            end
        u = strtod(v);
        xinfo("Intensity = "+u+" a.u.");
        i = i+1;
        A(i,1)= u;
        disp(u);
    values=[]
    value=ascii(0);
    updateSensorValue(data);
    end
endfunction

function stop_button()
    global Acquisition
    global A
    Acquisition = %f;
    disp(A);
endfunction

function print_button()
    global A
    stop_button();
    fprintfMat(uigetfile(), A);
endfunction

function quit_button()
    global serial_port
        closeserial(serial_port);
    f = findobj("tag", "mainWindow");
    delete(f);
endfunction

function updateSensorValue(data)
    e = findobj("tag", "minuteSensor");
    lastPoints = e.data(:, 2);
    e.data(:, 2) = [lastPoints(2:$) ; data];
endfunction

function reset_button()
    global A
    e = findobj("tag", "minuteSensor");
    e.data(:, 2) = 0;
    A = [];
endfunction

function motorForward()                                                                                                                                                                                                 
    global serial_port
    if  flag == 2 then
        serialwrite(serial_port,'H');
    elseif flag == 1 | flag == 3 then
        writeserial(serial_port,ascii(72));
    end
endfunction
//
function motorBackward()
    global serial_port
    if  flag == 2 then
        serialwrite(serial_port,ascii(76));
    elseif flag == 1 | flag == 3 then
        writeserial(serial_port,"L");
    end
endfunction

////////////////////////////PLOT////////////////////////////////////////////////

top_axes_bounds = [0.25 0 0.8 0.5];
timeBuffer = 300;
subplot(222);
a = gca();
a.axes_bounds = top_axes_bounds;
a.tag = "minuteAxes";
plot2d(0:timeBuffer, zeros(1,timeBuffer + 1), color("red"));
a.title.text="Intensity variations in the last 5 minutes";
a.data_bounds = [0, 0; timeBuffer, 1023];
e = gce();
e = e.children(1);
e.tag = "minuteSensor";

////////////////////////////UI Control/////////////////////////////////////////

mainFrame = uicontrol(f, "style", "frame", "position", [15 520 305 120], ...
"tag", "mainFrame", "ForegroundColor", [0/255 0/255 0/255],...
"border", createBorder("titled", createBorder("line", "lightGray", 1)...
, _("Main Panel"), "center", "top", createBorderFont("", 11, "normal"), ...
"black"));
//
startButton = uicontrol(f, "style", "pushbutton", "position", ...
[20 595 145 30], "callback", "start_button", "string", "Start Acquisition", ...
"tag", "startButton");
//
stopButton  = uicontrol(f, "style", "pushbutton", "position", ...
[170 595 145 30], "callback", "stop_button", "string", "Stop Acquisition", ...
"tag", "stopButton");
//
quitButton = uicontrol(f, "style", "pushbutton", "position", ...
[170 565 145 30], "callback", "quit_button", "string", "Quit", ...
"tag", "quitButton");
//
printButton = uicontrol(f, "style", "pushbutton", "position", ...
[20 565 145 30], "callback", "print_button", "string", "Print", ...
"tag", "printButton");
//
printButton = uicontrol(f, "style", "pushbutton", "position", ...
[20 535 145 30], "callback", "reset_button", "string", "Reset", ...
"tag", "resetButton");

textProgress = uicontrol(f, "style", "text", "units", "normalized", "position", [0 0.5 0.5 0.5], "string", "Label");
