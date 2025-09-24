/// +------------------------------------------------------------------------------------------------------------------------------+
/// |                                                   TERMS OF USE: MIT License                                                  |
/// +------------------------------------------------------------------------------------------------------------------------------|
/// |Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation    |
/// |files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,    |
/// |modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software|
/// |is furnished to do so, subject to the following conditions:                                                                   |
/// |                                                                                                                              |
/// |The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.|
/// |                                                                                                                              |
/// |THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE          |
/// |WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR         |
/// |COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,   |
/// |ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.                         |
/// +------------------------------------------------------------------------------------------------------------------------------+

/// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
///  PRU1_StepperIO.p - a PASM assembly language program, intended to run in
///                     the Beaglebone Black PRU1 which will send strictly
///                     defined waveforms and direction signals to up to 
///                     6 stepper motors.
///
///                     This code uses almost all of the I/O capabilities of the
///                     PRU1 and it must be run in a headless environment due
///                     to PimMux collisions with the output lines.
///
///                     NOTE: you must set the following pins to output
///                         for the signal and stepper direction lines
///                         P8_46, P8_45, P8_43, P8_44, P8_41, P8_42, 
///                         P8_39, P8_40, P8_27, P8_28, P8_29, P8_30
///
///                     Below is a subsection of the relevant device tree overlay
///                     
///                     0x0A0 0x25  /* P8_45 70   OUTPUT MODE5 - pr1_pru1_pru_r30_0 */
///                     0x0A4 0x25  /* P8_46 71   OUTPUT MODE5 - pr1_pru1_pru_r30_1 */
///                     0x0A8 0x25  /* P8_43 72   OUTPUT MODE5 - pr1_pru1_pru_r30_2 */
///                     0x0AC 0x25  /* P8_44 73   OUTPUT MODE5 - pr1_pru1_pru_r30_3 */
///                     0x0B0 0x25  /* P8_41 74   OUTPUT MODE5 - pr1_pru1_pru_r30_4 */
///                     0x0B4 0x25  /* P8_42 75   OUTPUT MODE5 - pr1_pru1_pru_r30_5 */
///                     0x0B8 0x25  /* P8_39 74   OUTPUT MODE5 - pr1_pru1_pru_r30_6 */
///                     0x0BC 0x25  /* P8_40 75   OUTPUT MODE5 - pr1_pru1_pru_r30_7 */
///                currently not used, steppers 4 and 5 removed
///                 //    0x0E0 0x25  /* P8_27 86   OUTPUT MODE5 - pr1_pru1_pru_r30_8 */
///                 //    0x0E8 0x25  /* P8_28 88   OUTPUT MODE5 - pr1_pru1_pru_r30_10 */
///                 //    0x0E4 0x25  /* P8_29 87   OUTPUT MODE5 - pr1_pru1_pru_r30_9 */
///                 //    0x0EC 0x25  /* P8_30 89   OUTPUT MODE5 - pr1_pru1_pru_r30_11 */

///                     The general mode of operation is to setup a series
///                     six blocks (one for each stepper) in which the path
///                     through the block, no matter which branches are taken,
///                     always takes exactly 10 statement executions. On the PRU's 
///                     each statement, as long as it does not access system
///                     memory, takes the same amount of time (5ns).
///
///                     This consistent timing means that various stepper motors
///                     can be enabled or disabled or have wildly varying pulse
///                     frequencies without any effect on the pulse widths of the
///                     other stepper motors.
///
///                     In order to keep things simple, we use the registers as
///                     variables. We do not have a lot of data to store and 
///                     this helps keeps the timings consistent too.
///
///                     Each stepper has an enable flag (0 or 1), a timing count
///                     which directly correlates to the frequency, and a 
///                     direction flag. There is also a global flag which turns 
///                     off all stepper motors and can generate a HALT on the PRU.
///
///                   Compile with the command
///                      pasm -b PRU1_StepperIO.p
/// 
///               References to the PRM refer to the AM335x PRU-ICSS Reference Guide
///               References to the TRM refer to the AM335x Sitara Processors
///                   Technical Reference Manual
///
///               Home Page
///                   http://www.OfItselfSo.com/Walnut/Walnut.php
/// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=

.origin 0
.entrypoint START

// this defines the data RAM memory location inside the PRUs address space. 
#define PRU_DATARAM 0        // yep, starts at address zero no matter which PRU you are using
#define BYTES_IN_REG 4       // there are 4 bytes in a register. This is only
                             // defined as a constant so its use is obvious
                             // in the BYTES_IN_CLIENT_DATA (and other) values below

#define DEFAULT_HALFCYCLEFULLCOUNT 5000000 // should not (or ever) be zero
#define DEFAULT_STARTSTATE 0 
#define DEFAULT_ENABLEDSTATE 0
#define INFINITE_PULSES_VALUE 0xFFFFFFFF
#define DEFAULT_CURRENTNUMPULSESCOUNT 0

// Note we do NOT need to enable the OCP master port. This is because this 
// code does not read directly out to the BBB's userspace memory. It reads 
// from its own data RAM and this is presented to userspace as a MemoryMapped 
// file by the UIO driver.

// these define the pins we update for the various outputs
#define STEP0_OUTPUTREG r30.t0           // P8_45 the bit in R30 we toggle to set the state
#define STEP0_DIRREG    r30.t1           // P8_46 the bit in R30 we toggle to set the direction
#define STEP1_OUTPUTREG r30.t2           // P8_43 the bit in R30 we toggle to set the state
#define STEP1_DIRREG    r30.t3           // P8_44 the bit in R30 we toggle to set the direction
#define STEP2_OUTPUTREG r30.t4           // P8_41 the bit in R30 we toggle to set the state
#define STEP2_DIRREG    r30.t5           // P8_42 the bit in R30 we toggle to set the direction
#define STEP3_OUTPUTREG r30.t6           // P8_39 the bit in R30 we toggle to set the state
#define STEP3_DIRREG    r30.t7           // P8_40 the bit in R30 we toggle to set the direction

#define OUT_AND_DIR_BITREG R30           // the register we use for all output state bits (see above)

// these registers are used as temporary variables and the code updates them dynamically

#define INFINITE_PULSES_FLAG        R2              // contains a value which indicates if we 
                                                    // should do infinite steps (0xFFFFFFFF)
#define DATA_COPY_REG               R3              // register we use when copying data from 
                                                    // client to PRU and vice versa

#define STEP0_CURRENTHALFCYCLECOUNT R4              // downcounter, 0 means toggle line state
#define STEP1_CURRENTHALFCYCLECOUNT R5              // downcounter, 0 means toggle line state
#define STEP2_CURRENTHALFCYCLECOUNT R6              // downcounter, 0 means toggle line state
#define STEP3_CURRENTHALFCYCLECOUNT R7              // downcounter, 0 means toggle line state

#define STEP0_CURRENTNUMPULSESCOUNT R8              // upcounter, ge MAX_NUMPULSES means stop
#define STEP1_CURRENTNUMPULSESCOUNT R9              // upcounter, ge MAX_NUMPULSES means stop
#define STEP2_CURRENTNUMPULSESCOUNT R10             // upcounter, ge MAX_NUMPULSES means stop
#define STEP3_CURRENTNUMPULSESCOUNT R11             // upcounter, ge MAX_NUMPULSES means stop

// the content of these registers is obtained from the client and, other than the semaphore,
// this code does not update them once they have been read. Think of them as temporary constants
// NOTE: the registers below MUST be sequential. They are loaded as a block. You can NEVER
// use the registers R30 and above

#define STATEFLAG_OFFSET 0               // this is an int which always contains the current contents
                                         // of r30 our PRUDriver can read this and know the current pin state
#define STEP0_NUMPULSES_OFFSET  4        // the code places a copy of STEP0_CURRENTNUMPULSESCOUNT in so the PRUDriver can read it
#define STEP1_NUMPULSES_OFFSET  8        // the code places a copy of STEP1_CURRENTNUMPULSESCOUNT in so the PRUDriver can read it
#define STEP2_NUMPULSES_OFFSET  12       // the code places a copy of STEP2_CURRENTNUMPULSESCOUNT in so the PRUDriver can read it
#define STEP3_NUMPULSES_OFFSET  16       // the code places a copy of STEP3_CURRENTNUMPULSESCOUNT in so the PRUDriver can read it

// this is the sum of the count of the bytes in all of the registers from STEP0_NUMPULSES_OFFSET to STEP3_NUMPULSES_OFFSET
#define BYTES_IN_PULSEDATA_WE_WRITE_BACK (4 * BYTES_IN_REG)   // the total number of bytes of write back data 

#define STEP0_ENASTATE_OFFSET  20
#define STEP1_ENASTATE_OFFSET  24
#define STEP2_ENASTATE_OFFSET  28
#define STEP3_ENASTATE_OFFSET  32

// this is the sum of the count of the bytes in all of the registers from STEP0_ENASTATE_OFFSET to STEP3_ENASTATE_OFFSET
#define BYTES_IN_ENA_DATA_WE_WRITE_BACK (4 * BYTES_IN_REG)   // the total number of bytes of write back data 

#define SEMAPHORE_OFFSET 36              // this is the offset of the semaphore data in the clients dataspace

#define SEMAPHORE_REG   R12              // 0 - no data to read, nz - data can be read
                                         // this is the last byte set by the client
                                         // when it has updated the freq and dir data for the steppers
                                         // are moved into the registers here. The bit set indicates the
                                         // steppers that changed

#define STEPPER0_CHANGED SEMAPHORE_REG.t0   // a bit flag indicating that stepper0 changed
#define STEPPER1_CHANGED SEMAPHORE_REG.t1   // a bit flag indicating that stepper1 changed
#define STEPPER2_CHANGED SEMAPHORE_REG.t2   // a bit flag indicating that stepper2 changed
#define STEPPER3_CHANGED SEMAPHORE_REG.t3   // a bit flag indicating that stepper3 changed

#define STEPALL_ENABLED R13              // 0 all steppers disabled, 1 steppers can be enabled
                                         // anything other than 0 or 1 means clear all outputs
                                         // clear all dir pins and HALT the PRU
#define STEP0_ENABLED   R14              // 0 disabled, 1 enabled
#define STEP1_ENABLED   R15              // 0 disabled, 1 enabled
#define STEP2_ENABLED   R16              // 0 disabled, 1 enabled
#define STEP3_ENABLED   R17              // 0 disabled, 1 enabled

#define STEP0_FULLHALFCYCLECOUNT R18     // this is the count we reset to when we toggle
#define STEP1_FULLHALFCYCLECOUNT R19     // this is the count we reset to when we toggle
#define STEP2_FULLHALFCYCLECOUNT R20     // this is the count we reset to when we toggle
#define STEP3_FULLHALFCYCLECOUNT R21     // this is the count we reset to when we toggle

#define STEP0_DIRSTATE  R22              // this is the state of the direction pin
#define STEP1_DIRSTATE  R23              // this is the state of the direction pin
#define STEP2_DIRSTATE  R24              // this is the state of the direction pin
#define STEP3_DIRSTATE  R25              // this is the state of the direction pin

#define STEP0_MAX_NUMPULSES   R26        // Max mumber of pulses we send, 0xFFFFFFFF for infinite
#define STEP1_MAX_NUMPULSES   R27        // Max mumber of pulses we send, 0xFFFFFFFF for infinite
#define STEP2_MAX_NUMPULSES   R28        // Max mumber of pulses we send, 0xFFFFFFFF for infinite
#define STEP3_MAX_NUMPULSES   R29        // Max mumber of pulses we send, 0xFFFFFFFF for infinite


// this is the sum of the count of the bytes in all of the registers from SEMAPHORE_REG to STEP3_MAX_NUMPULSES
#define BYTES_IN_CLIENT_DATA (18 * BYTES_IN_REG)   // the total number of bytes in the client data 

             // this label is where the code execution starts
START:

             // initialize
INIT:        CLR  STEP0_OUTPUTREG
             MOV  STEP0_ENABLED,   DEFAULT_ENABLEDSTATE
             MOV  STEP0_FULLHALFCYCLECOUNT, DEFAULT_HALFCYCLEFULLCOUNT
             MOV  STEP0_CURRENTHALFCYCLECOUNT, STEP0_FULLHALFCYCLECOUNT
             MOV  STEP0_CURRENTNUMPULSESCOUNT, DEFAULT_CURRENTNUMPULSESCOUNT  

			 CLR  STEP1_OUTPUTREG
             MOV  STEP1_ENABLED,   DEFAULT_ENABLEDSTATE
             MOV  STEP1_FULLHALFCYCLECOUNT, DEFAULT_HALFCYCLEFULLCOUNT
             MOV  STEP1_CURRENTHALFCYCLECOUNT, STEP1_FULLHALFCYCLECOUNT
             MOV  STEP1_CURRENTNUMPULSESCOUNT, DEFAULT_CURRENTNUMPULSESCOUNT  

			 CLR  STEP2_OUTPUTREG
             MOV  STEP2_ENABLED,   DEFAULT_ENABLEDSTATE
             MOV  STEP2_FULLHALFCYCLECOUNT, DEFAULT_HALFCYCLEFULLCOUNT
             MOV  STEP2_CURRENTHALFCYCLECOUNT, STEP2_FULLHALFCYCLECOUNT
             MOV  STEP2_CURRENTNUMPULSESCOUNT, DEFAULT_CURRENTNUMPULSESCOUNT  

			 CLR  STEP3_OUTPUTREG
             MOV  STEP3_ENABLED,   DEFAULT_ENABLEDSTATE
             MOV  STEP3_FULLHALFCYCLECOUNT, DEFAULT_HALFCYCLEFULLCOUNT
             MOV  STEP3_CURRENTHALFCYCLECOUNT, STEP3_FULLHALFCYCLECOUNT
             MOV  STEP3_CURRENTNUMPULSESCOUNT, DEFAULT_CURRENTNUMPULSESCOUNT  

             // this is compared against to see if we are doing infinite pulses
             MOV INFINITE_PULSES_FLAG, INFINITE_PULSES_VALUE

// The top of the loop
LOOP_TOP:      

// there is one block below for each Stepper motor. The code is
// structured in such a way that no matter which path is taken
// through it, the processing takes 10 instructions. This keeps 
// timings consistent. The various steppers can have different
// frequencies, be enabled or disabled, and the processing time
// through the block identical for each

             // #######
             // ####### STEP0 specific actions
             // #######
STEP0:       QBEQ STEP0_TEST, STEP0_ENABLED, 1        // is STEP0_ENABLED == 1? if yes, then toggle    
             CLR  STEP0_OUTPUTREG                     // not enabled, clear the pin 
             JMP  STEP0_NOP09                          
STEP0_TEST:  SUB  STEP0_CURRENTHALFCYCLECOUNT, STEP0_CURRENTHALFCYCLECOUNT, 1 // decrement the count
             QBNE STEP0_NOP08, STEP0_CURRENTHALFCYCLECOUNT, 0      // is the downcount == 0? if no, then NOPout
             MOV  STEP0_CURRENTHALFCYCLECOUNT, STEP0_FULLHALFCYCLECOUNT    // reset the count now
STEP0_TOGG:  QBBC STEP0_PULSE, STEP0_OUTPUTREG        // we need to toggle, are we currently high?
             // yes, we are currently high
STEP0_LOW:   CLR  STEP0_OUTPUTREG                     // clear the pin 
             JMP  STEP0_NOP06
             // Now we check to see if we have done enough cycles
             // first check to see if we need to care about this
STEP0_PULSE: QBEQ STEP0_HIGH1, STEP0_MAX_NUMPULSES, INFINITE_PULSES_FLAG
             // we do need to care, if STEP0_CURRENTNUMPULSESCOUNT >= STEP0_MAX_NUMPULSES
             // we quit otherwise we just count it and set the pin high
             QBLT STEP0_HIGH, STEP0_MAX_NUMPULSES, STEP0_CURRENTNUMPULSESCOUNT
             // yes, we have reached the limit, disable
             MOV  STEP0_ENABLED, 0                    // now not enabled
             CLR  STEP0_OUTPUTREG                     // clear the pin for sure
             JMP  STEP0_NOP00
             // need a nop for the timings
STEP0_HIGH1: MOV  R0, R0                              // just a NOP
             // count it always
STEP0_HIGH:  ADD  STEP0_CURRENTNUMPULSESCOUNT, STEP0_CURRENTNUMPULSESCOUNT, 1
             SET  STEP0_OUTPUTREG                     // set the pin 
             JMP  STEP0_NOP00
STEP0_NOP09: MOV  R0, R0                              // just a NOP
STEP0_NOP08: MOV  R0, R0                              // just a NOP
STEP0_NOP07: MOV  R0, R0                              // just a NOP
STEP0_NOP06: MOV  R0, R0                              // just a NOP
STEP0_NOP05: MOV  R0, R0                              // just a NOP
STEP0_NOP04: MOV  R0, R0                              // just a NOP
STEP0_NOP03: MOV  R0, R0                              // just a NOP
STEP0_NOP02: MOV  R0, R0                              // just a NOP
STEP0_NOP01: MOV  R0, R0                              // just a NOP
STEP0_NOP00: MOV  R0, R0                              // just a NOP

             // #######
             // ####### STEP1 specific actions
             // #######
STEP1:       QBEQ STEP1_TEST, STEP1_ENABLED, 1        // is STEP1_ENABLED == 1? if yes, then toggle    
             CLR  STEP1_OUTPUTREG                     // not enabled, clear the pin 
             JMP  STEP1_NOP09                          
STEP1_TEST:  SUB  STEP1_CURRENTHALFCYCLECOUNT, STEP1_CURRENTHALFCYCLECOUNT, 1 // decrement the count
             QBNE STEP1_NOP08, STEP1_CURRENTHALFCYCLECOUNT, 0      // is the downcount == 0? if no, then NOPout
             MOV  STEP1_CURRENTHALFCYCLECOUNT, STEP1_FULLHALFCYCLECOUNT    // reset the count now
STEP1_TOGG:  QBBC STEP1_PULSE, STEP1_OUTPUTREG        // we need to toggle, are we currently high?
             // yes, we are currently high
STEP1_LOW:   CLR  STEP1_OUTPUTREG                     // clear the pin 
             JMP  STEP1_NOP06
             // Now we check to see if we have done enough cycles
             // first check to see if we need to care about this
STEP1_PULSE: QBEQ STEP1_HIGH1, STEP1_MAX_NUMPULSES, INFINITE_PULSES_FLAG
             // we do need to care, if STEP1_CURRENTNUMPULSESCOUNT >= STEP1_MAX_NUMPULSES
             // we quit otherwise we just count it and set the pin high
             QBLT STEP1_HIGH, STEP1_MAX_NUMPULSES, STEP1_CURRENTNUMPULSESCOUNT
             // yes, we have reached the limit, disable
             MOV  STEP1_ENABLED, 0                    // now not enabled
             CLR  STEP1_OUTPUTREG                     // clear the pin for sure
             JMP  STEP1_NOP00
             // need a nop for the timings
STEP1_HIGH1: MOV  R0, R0                              // just a NOP
             // count it always
STEP1_HIGH:  ADD  STEP1_CURRENTNUMPULSESCOUNT, STEP1_CURRENTNUMPULSESCOUNT, 1
             SET  STEP1_OUTPUTREG                     // set the pin 
             JMP  STEP1_NOP00
STEP1_NOP09: MOV  R0, R0                              // just a NOP
STEP1_NOP08: MOV  R0, R0                              // just a NOP
STEP1_NOP07: MOV  R0, R0                              // just a NOP
STEP1_NOP06: MOV  R0, R0                              // just a NOP
STEP1_NOP05: MOV  R0, R0                              // just a NOP
STEP1_NOP04: MOV  R0, R0                              // just a NOP
STEP1_NOP03: MOV  R0, R0                              // just a NOP
STEP1_NOP02: MOV  R0, R0                              // just a NOP
STEP1_NOP01: MOV  R0, R0                              // just a NOP
STEP1_NOP00: MOV  R0, R0                              // just a NOP

             // #######
             // ####### STEP2 specific actions
             // #######
STEP2:       QBEQ STEP2_TEST, STEP2_ENABLED, 1        // is STEP2_ENABLED == 1? if yes, then toggle    
             CLR  STEP2_OUTPUTREG                     // not enabled, clear the pin 
             JMP  STEP2_NOP09                          
STEP2_TEST:  SUB  STEP2_CURRENTHALFCYCLECOUNT, STEP2_CURRENTHALFCYCLECOUNT, 1 // decrement the count
             QBNE STEP2_NOP08, STEP2_CURRENTHALFCYCLECOUNT, 0      // is the downcount == 0? if no, then NOPout
             MOV  STEP2_CURRENTHALFCYCLECOUNT, STEP2_FULLHALFCYCLECOUNT    // reset the count now
STEP2_TOGG:  QBBC STEP2_PULSE, STEP2_OUTPUTREG        // we need to toggle, are we currently high?
             // yes, we are currently high
STEP2_LOW:   CLR  STEP2_OUTPUTREG                     // clear the pin 
             JMP  STEP2_NOP06
             // Now we check to see if we have done enough cycles
             // first check to see if we need to care about this
STEP2_PULSE: QBEQ STEP2_HIGH1, STEP2_MAX_NUMPULSES, INFINITE_PULSES_FLAG
             // we do need to care, if STEP2_CURRENTNUMPULSESCOUNT >= STEP2_MAX_NUMPULSES
             // we quit otherwise we just count it and set the pin high
             QBLT STEP2_HIGH, STEP2_MAX_NUMPULSES, STEP2_CURRENTNUMPULSESCOUNT
             // yes, we have reached the limit, disable
             MOV  STEP2_ENABLED, 0                    // now not enabled
             CLR  STEP2_OUTPUTREG                     // clear the pin for sure
             JMP  STEP2_NOP00
             // need a nop for the timings
STEP2_HIGH1: MOV  R0, R0                              // just a NOP
             // count it always
STEP2_HIGH:  ADD  STEP2_CURRENTNUMPULSESCOUNT, STEP2_CURRENTNUMPULSESCOUNT, 1
             SET  STEP2_OUTPUTREG                     // set the pin 
             JMP  STEP2_NOP00
STEP2_NOP09: MOV  R0, R0                              // just a NOP
STEP2_NOP08: MOV  R0, R0                              // just a NOP
STEP2_NOP07: MOV  R0, R0                              // just a NOP
STEP2_NOP06: MOV  R0, R0                              // just a NOP
STEP2_NOP05: MOV  R0, R0                              // just a NOP
STEP2_NOP04: MOV  R0, R0                              // just a NOP
STEP2_NOP03: MOV  R0, R0                              // just a NOP
STEP2_NOP02: MOV  R0, R0                              // just a NOP
STEP2_NOP01: MOV  R0, R0                              // just a NOP
STEP2_NOP00: MOV  R0, R0                              // just a NOP

             // #######
             // ####### STEP3 specific actions
             // #######
STEP3:       QBEQ STEP3_TEST, STEP3_ENABLED, 1        // is STEP3_ENABLED == 1? if yes, then toggle    
             CLR  STEP3_OUTPUTREG                     // not enabled, clear the pin 
             JMP  STEP3_NOP09                          
STEP3_TEST:  SUB  STEP3_CURRENTHALFCYCLECOUNT, STEP3_CURRENTHALFCYCLECOUNT, 1 // decrement the count
             QBNE STEP3_NOP08, STEP3_CURRENTHALFCYCLECOUNT, 0      // is the downcount == 0? if no, then NOPout
             MOV  STEP3_CURRENTHALFCYCLECOUNT, STEP3_FULLHALFCYCLECOUNT    // reset the count now
STEP3_TOGG:  QBBC STEP3_PULSE, STEP3_OUTPUTREG        // we need to toggle, are we currently high?
             // yes, we are currently high
STEP3_LOW:   CLR  STEP3_OUTPUTREG                     // clear the pin 
             JMP  STEP3_NOP06
             // Now we check to see if we have done enough cycles
             // first check to see if we need to care about this
STEP3_PULSE: QBEQ STEP3_HIGH1, STEP3_MAX_NUMPULSES, INFINITE_PULSES_FLAG
             // we do need to care, if STEP3_CURRENTNUMPULSESCOUNT >= STEP3_MAX_NUMPULSES
             // we quit otherwise we just count it and set the pin high
             QBLT STEP3_HIGH, STEP3_MAX_NUMPULSES, STEP3_CURRENTNUMPULSESCOUNT
             // yes, we have reached the limit, disable
             MOV  STEP3_ENABLED, 0                    // now not enabled
             CLR  STEP3_OUTPUTREG                     // clear the pin for sure
             JMP  STEP3_NOP00
             // need a nop for the timings
STEP3_HIGH1: MOV  R0, R0                              // just a NOP
             // count it always
STEP3_HIGH:  ADD  STEP3_CURRENTNUMPULSESCOUNT, STEP3_CURRENTNUMPULSESCOUNT, 1
             SET  STEP3_OUTPUTREG                     // set the pin 
             JMP  STEP3_NOP00
STEP3_NOP09: MOV  R0, R0                              // just a NOP
STEP3_NOP08: MOV  R0, R0                              // just a NOP
STEP3_NOP07: MOV  R0, R0                              // just a NOP
STEP3_NOP06: MOV  R0, R0                              // just a NOP
STEP3_NOP05: MOV  R0, R0                              // just a NOP
STEP3_NOP04: MOV  R0, R0                              // just a NOP
STEP3_NOP03: MOV  R0, R0                              // just a NOP
STEP3_NOP02: MOV  R0, R0                              // just a NOP
STEP3_NOP01: MOV  R0, R0                              // just a NOP
STEP3_NOP00: MOV  R0, R0                              // just a NOP

// this section obtains the data from the Walnut client, and places it in the
// registers for use. The overhead of this is consistent and will
// not affect the frequency of the steppers since the timings are calibrated
// with it in place

STORE_DATA:  MOV  DATA_COPY_REG, PRU_DATARAM          // put the address of our 8Kb DataRAM space in DATA_COPY_REG
             // copy the pulse count registers to data out
             SBBO STEP0_CURRENTNUMPULSESCOUNT, DATA_COPY_REG, STEP0_NUMPULSES_OFFSET, BYTES_IN_PULSEDATA_WE_WRITE_BACK    
             // copy the enabled state registers to data out
             SBBO STEP0_ENABLED, DATA_COPY_REG, STEP0_ENASTATE_OFFSET, BYTES_IN_ENA_DATA_WE_WRITE_BACK    
             // copy the out and dir state register to data out
             SBBO OUT_AND_DIR_BITREG, DATA_COPY_REG, STATEFLAG_OFFSET, BYTES_IN_REG   


CHKPIN:      MOV  DATA_COPY_REG, PRU_DATARAM          // put the address of our 8Kb DataRAM space in DATA_COPY_REG
             MOV  SEMAPHORE_REG, 0                    // reset our local semaphore register
             LBBO SEMAPHORE_REG, DATA_COPY_REG, SEMAPHORE_OFFSET, BYTES_IN_REG    // read in just the semaphore
             QBEQ LOOP_TOP, SEMAPHORE_REG, 0          // is the semaphore set? if not, no new data, 
                                                      // carry on processing with what we have
                                                      
             // else, read in all of the client data, this writes to multiple contiguous registers
             LBBO SEMAPHORE_REG, DATA_COPY_REG, SEMAPHORE_OFFSET, BYTES_IN_CLIENT_DATA    // read in our client data
                                                                           
             // bits are set in the semaphore reg to indicate which steppers are changing. 
             // we only adjust and reset things on the changed ones
STEP0_CNG:   QBBC STEP1_CNG, STEPPER0_CHANGED
             MOV  STEP0_CURRENTNUMPULSESCOUNT, 0      // clear this count
             MOV  STEP0_CURRENTHALFCYCLECOUNT, STEP0_FULLHALFCYCLECOUNT

STEP1_CNG:   QBBC STEP2_CNG, STEPPER1_CHANGED
             MOV  STEP1_CURRENTNUMPULSESCOUNT, 0      // clear this count
             MOV  STEP1_CURRENTHALFCYCLECOUNT, STEP1_FULLHALFCYCLECOUNT

STEP2_CNG:   QBBC STEP3_CNG, STEPPER2_CHANGED
             MOV  STEP2_CURRENTNUMPULSESCOUNT, 0      // clear this count
             MOV  STEP2_CURRENTHALFCYCLECOUNT, STEP2_FULLHALFCYCLECOUNT

STEP3_CNG:   QBBC RESET_SEM, STEPPER3_CHANGED
             MOV  STEP3_CURRENTNUMPULSESCOUNT, 0      // clear this count
             MOV  STEP3_CURRENTHALFCYCLECOUNT, STEP3_FULLHALFCYCLECOUNT


             // now reset the semaphore and test
RESET_SEM:   MOV  SEMAPHORE_REG, 0                    // reset the semaphore reg
             SBBO SEMAPHORE_REG, DATA_COPY_REG, SEMAPHORE_OFFSET, BYTES_IN_REG    // reset the semaphore in memory
             QBEQ TEST_COUNT, STEPALL_ENABLED, 1      // all steppers enabled, processing can proceed
             QBEQ ALLLOW, STEPALL_ENABLED, 0          // not 0 or 1? this means exit
			 JMP  ALLSTOP

	    // here we test the full counts, they can never be zero, we do not permit this
TEST_COUNT: QBNE STEP0_FCOK, STEP0_FULLHALFCYCLECOUNT, 0       // is the fullcount 0?
            MOV  STEP0_ENABLED, 0                     // full count sent in as zero? disable stepper
STEP0_FCOK:                                           // no worries, full count is acceptable
            QBNE STEP1_FCOK, STEP1_FULLHALFCYCLECOUNT, 0       // is the fullcount 0?
            MOV  STEP1_ENABLED, 0                     // full count sent in as zero? disable stepper
STEP1_FCOK:                                           // no worries, full count is acceptable
            QBNE STEP2_FCOK, STEP2_FULLHALFCYCLECOUNT, 0       // is the fullcount 0?
            MOV  STEP2_ENABLED, 0                     // full count sent in as zero? disable stepper
STEP2_FCOK:                                           // no worries, full count is acceptable
            QBNE STEP3_FCOK, STEP3_FULLHALFCYCLECOUNT, 0       // is the fullcount 0?
            MOV  STEP3_ENABLED, 0                     // full count sent in as zero? disable stepper
STEP3_FCOK:                                           // no worries, full count is acceptable

        // here we test the current full cycle count is not greater than the new full cycle count
		// this can happen if the user increases the speed suddenly, we do not want
		// to have to wait for the current cycle to complete before the new count kicks in
            QBLT STEP0_DCOK, STEP0_FULLHALFCYCLECOUNT, STEP0_CURRENTHALFCYCLECOUNT    // is the downcount < fullcount?
            MOV  STEP0_CURRENTHALFCYCLECOUNT, STEP0_FULLHALFCYCLECOUNT     // reset the downcount to the new maximum
STEP0_DCOK:                                           // no worries, down count is acceptable
            QBLT STEP1_DCOK, STEP1_FULLHALFCYCLECOUNT, STEP1_CURRENTHALFCYCLECOUNT    // is the downcount < fullcount?
            MOV  STEP1_CURRENTHALFCYCLECOUNT, STEP1_FULLHALFCYCLECOUNT     // reset the downcount to the new maximum
STEP1_DCOK:                                           // no worries, down count is acceptable
            QBLT STEP2_DCOK, STEP2_FULLHALFCYCLECOUNT, STEP2_CURRENTHALFCYCLECOUNT    // is the downcount < fullcount?
            MOV  STEP2_CURRENTHALFCYCLECOUNT, STEP2_FULLHALFCYCLECOUNT     // reset the downcount to the new maximum
STEP2_DCOK:                                           // no worries, down count is acceptable
            QBLT STEP3_DCOK, STEP3_FULLHALFCYCLECOUNT, STEP3_CURRENTHALFCYCLECOUNT    // is the downcount < fullcount?
            MOV  STEP3_CURRENTHALFCYCLECOUNT, STEP3_FULLHALFCYCLECOUNT     // reset the downcount to the new maximum
STEP3_DCOK:                                           // no worries, down count is acceptable

        // here we set the direction pins
STEP0_SDIR:  QBEQ STEP0_DLOW, STEP0_ENABLED, 0        // not enabled, set it low    
             QBEQ STEP0_DLOW, STEP0_DIRSTATE, 0       // what does the dir state say?
             SET  STEP0_DIRREG                        // it is nz, set the pin
			 JMP  STEP0_DEND
STEP0_DLOW:  CLR  STEP0_DIRREG                        // not enabled, clear the pin 
STEP0_DEND:                                           // the end of the STEP0 direction pin 

STEP1_SDIR:  QBEQ STEP1_DLOW, STEP1_ENABLED, 0        // not enabled, set it low    
             QBEQ STEP1_DLOW, STEP1_DIRSTATE, 0       // what does the dir state say?
             SET  STEP1_DIRREG                        // it is nz, set the pin
			 JMP  STEP1_DEND
STEP1_DLOW:  CLR  STEP1_DIRREG                        // not enabled, clear the pin 
STEP1_DEND:                                           // the end of the STEP1 direction pin 

STEP2_SDIR:  QBEQ STEP2_DLOW, STEP2_ENABLED, 0        // not enabled, set it low    
             QBEQ STEP2_DLOW, STEP2_DIRSTATE, 0       // what does the dir state say?
             SET  STEP2_DIRREG                        // it is nz, set the pin
			 JMP  STEP2_DEND
STEP2_DLOW:  CLR  STEP2_DIRREG                        // not enabled, clear the pin 
STEP2_DEND:                                           // the end of the STEP2 direction pin 

STEP3_SDIR:  QBEQ STEP3_DLOW, STEP3_ENABLED, 0        // not enabled, set it low    
             QBEQ STEP3_DLOW, STEP3_DIRSTATE, 0       // what does the dir state say?
             SET  STEP3_DIRREG                        // it is nz, set the pin
			 JMP  STEP3_DEND
STEP3_DLOW:  CLR  STEP3_DIRREG                        // not enabled, clear the pin 
STEP3_DEND:                                           // the end of the STEP3 direction pin 
             JMP LOOP_TOP                             // go back to the start

        // anything else, we put the pin low and stop
ALLSTOP:    CLR  STEP0_DIRREG                         // clear the direction
            CLR  STEP0_OUTPUTREG                      // clear the output state
			CLR  STEP1_DIRREG                         // clear the direction
            CLR  STEP1_OUTPUTREG                      // clear the output state
			CLR  STEP2_DIRREG                         // clear the direction
            CLR  STEP2_OUTPUTREG                      // clear the output state
 			CLR  STEP3_DIRREG                         // clear the direction
            CLR  STEP3_OUTPUTREG                      // clear the output state
            HALT                                      // stop the PRU
            
        // all steppers disabled, set all steppers low
ALLLOW:     CLR  STEP0_DIRREG                         // clear the direction
            CLR  STEP0_OUTPUTREG                      // clear the output state
			CLR  STEP1_DIRREG                         // clear the direction
            CLR  STEP1_OUTPUTREG                      // clear the output state
			CLR  STEP2_DIRREG                         // clear the direction
            CLR  STEP2_OUTPUTREG                      // clear the output state
			CLR  STEP3_DIRREG                         // clear the direction
            CLR  STEP3_OUTPUTREG                      // clear the output state
            JMP  CHKPIN                               // look for the re-enable
