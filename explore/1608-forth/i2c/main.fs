\ application setup and main loop
\ detect and list all attached I2C devices periodically

0 constant DEBUG  \ 0 = show on LCD, 1 = show on serial

\ debug LEDs, connected to rightmost I/O pins on main header
PA0  constant LED1
PA1  constant LED2
PA2  constant LED3
PA3  constant LED4
PA11 constant LED5
PA12 constant LED6

: leds-pwm ( -- )  \ blink the attached LEDs, just because we can...
\ FIXME set alt function #2 on all PWM pins, should be moved inside pwm driver
  $00002222 LED1 io-base GPIO.AFRL + !

  \ various duty cycles at 2 Hz
  2 LED1 +pwm   500 LED1 pwm
  2 LED2 +pwm  3500 LED2 pwm
  2 LED3 +pwm  6500 LED3 pwm
  2 LED4 +pwm  9500 LED4 pwm
;

: lcd-emit ( c -- )  \ switch the output to the OLED, cr's move to next line
  dup $0A = if drop
    s"                 "  \ dumb way to clear a line
    0 dup font-x !  font-y @  8 +  $38 and  dup font-y !
    drawstring
    0 font-x !
  else
    ascii>bitpattern drawcharacterbitmap
  then ;

\ single-digit hex output
: h.1 ( u -- ) $F and base @ hex swap  .digit emit  base ! ;

: i2c.short ( -- )  \ scan and report all I2C devices on the bus, short format
  128 0 do
    DEBUG if  cr i h.2 ." : "  else  i if cr then  then
    
    16 0 do
      i j +
      dup $08 < over $77 > or if drop space else
        dup i2c-addr  0 i2c-xfer  if drop ." -" else h.1 then
      then
    loop
  16 +loop ;

: main
  leds-pwm  +i2c lcd-init show-logo
  DEBUG 0= if  ['] lcd-emit hook-emit !  then

  8686 rf69.freq ! 6 rf69.group ! 62 rf69.nodeid !
  rf69-init 16 rf-power rf-sleep

  begin
    1000 ms
    cr 0 font-x ! 0 font-y ! clear i2c.short display
  key? until

  ['] serial-emit hook-emit ! ;