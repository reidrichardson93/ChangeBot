using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ConsoleApp2
{
    class Program
    {
        //A global dictionary will be created to link each denomination to a count value
        static Dictionary<string, int> till = new Dictionary<string, int>();
        static Random rnd = new Random();

        static void Main(string[] args)
        {
            
            //Two queues have been initialized to keep track of what the user enters into the till and the possible change returned
            //card output will hold onto (most importantly) the amount of the balance that has been paid so far
            Queue<string> cash_inserted = new Queue<string>();
            Queue<string> cash_returned = new Queue<string>();
            string[] card_output = new string[4];
            int transactionNumber = 0;

            //This dictionary will then be initialized with a realistic large number of "bills"
            //A dictionary with a string key and an int value "till" initialized with a set
            //number of each denomination, the start up cash
            //Till initialization process sent to tillInitializer function
            bool dump = Initialize_Till(5, 10, 200, 400, 500, 10, 1000, 10, 10, 1000, 1000, 2000, 2000);

            //This program will have a master loop that keeps it open so the money count will not reset
                //Create a bool variable "loop" that is initialized to true
                //Open a while loop with loop as the condition so the program will be on an endless loop
            decimal total = 0.00m;
            decimal changeDue = 0.00m;
            decimal cashTender = 0.00m;
            decimal totalTender = 0.00m;
            bool loop = true;
            while (loop){
                transactionNumber++;
                
                //Next, the user will be prompted to input the prices of their items that will be stored in a list
                //The input will be checked and validated after each price entry
                    //The validated "input" will then be parsed to a decimal and added to "total"
                    //initializing car_output[1] that stores the amount paid so far
                total = 0.00m;
                cashTender = 0.00m;
                changeDue = 0.00m;
                totalTender = 0.00m;
                string input = "0.00";
                card_output[0] = "";
                card_output[1] = "0.00";
                card_output[2] = "N/A";
                card_output[3] = "0.00";
                int count = 1;
                while (input != ""){
                    input = Prompt("Item " + count);
                    if (input != ""){
                        total += decimal.Parse(input);
                    }else{
                        //User has entered an empty string meaning they are done entering items
                        //total has been calculated and is now being dispalyed to screen
                        Console.WriteLine("\nTotal    $" + total.ToString("F"));
                        Console.WriteLine();
                    }
                    count++;
                }
                //payment loop that is only broken when the payment has been completed successfully
                while (total > totalTender)
                {
                    //prompting if the customer wants to pay with cash or card using cashOrCard funciton
                    bool cash = cashOrCard();
                    //if the user selects cash
                    if (cash)
                    {
                        //the enterCash function is called being sent the remaining due and a queue to store the inserted bills
                        //enterCash function calculates and returns the changeDue based on the amount of cash entered
                        changeDue = enterCash(total - decimal.Parse(card_output[1]), cash_inserted);

                        //changeBreaker function is called being sent the changeDue and a queue to store the bills to be returned
                        //returns a bool based on if the kiosk has enough cash to return to the customer
                        bool breakable = changeBreaker(changeDue, cash_returned);

                        //if the change is breakable
                        if (breakable)
                        {

                            //the change dispenser is called to display all of the bills that were returned to the customer
                            //funciton takes the cash_returned queue that was populated by change_breaker
                            changeDispenser(cash_returned);

                            //sets the card_output[1] to total in order to break out of the payment loop
                            cashTender = total + changeDue - (decimal.Parse(card_output[1]));
                            totalTender += cashTender;
                            //else the change is not breakable
                        }
                        else
                        {

                            //display unbreakable message, then return the till back to before the transaction by calling tenderReturn
                            //tenderReturn takes the queues that stored the bill exchange in and out
                            //customer is then asked to use a different payment method
                            Console.WriteLine("\nThis kiosk does not have enough change. Your money is being returned.");
                            tenderReturn(cash_inserted, cash_returned);
                            Console.WriteLine("Please complete this transaction with a Card.");
                        }
                        //else the user selects to use a card
                    }
                    else
                    {
                        //call the cardReader function that takes the total price and returns a string array
                        //the string array holds the card number and the ammount paid so far
                        card_output = cardReader(total);

                        //once the card has been run we check the output

                        //if the cardReader returns 'declined' or if the ammount paid is not greater than the total
                        //this if checks whether or not the transaction has been completed
                        //the ammount paid could be greater than the total because this kiosk offers cash back
                        if (((card_output[1] == "declined") || (card_output[1] == "")) || (decimal.Parse(card_output[1]) < total))
                        {
                            //incomplete transaction
                            //nested if to check if the card was declined
                            if (card_output[1] == "declined")
                            {
                                Console.WriteLine("Your card was declined.");
                                card_output[1] = "0.00";
                                //else if the card was invalid
                            }
                            else if (card_output[1] == "")
                            {
                                card_output[1] = "0.00";
                                //else the card was declined and we set card_output[1] to 0.00 as a decimal convertable string
                            }
                            else
                            {
                                Console.WriteLine("Your card has insufficient funds.");

                            }
                            //Instruct customer to use a diffrent payment method
                            Console.WriteLine("Please pay with a different card or cash.");
                            Console.WriteLine("\nRemaining due: ${0}\n", ((total - decimal.Parse(card_output[1])).ToString("F")));
                            //else the transaction was completed and the checkout/payment loop is finished
                        }
                        else
                        {
                            Console.WriteLine("Transaction successful!");
                        }
                        //keeping track of total tender for transaction log
                        totalTender += decimal.Parse(card_output[1]);
                    }
                }
                //calling createHeader to properly format the heading info for the transaction log
                //then compiling the transaction data into the expected format
                string header = createHeader(transactionNumber);
                string transactionData = "$" + cashTender + " " + card_output[2] + " $" + card_output[1] + " $" + (changeDue + decimal.Parse(card_output[3]));

                //calling outside program 'transactionLog.exe' in order to create a new log entry
                //calling the program then passing the args then starting the program
                ProcessStartInfo transactionLog = new ProcessStartInfo();
                transactionLog.FileName = "transactionLog.exe";
                transactionLog.Arguments = header + transactionData;
                Process.Start(transactionLog);

                //hold program waiting for the next transaction and reset the queues
                pause();
                queueClear(cash_inserted, cash_returned);
            }
        }//end main

        //pause function that prints a pause message, waitd for a key press, then clears the screen
        static void pause(){
            Console.WriteLine("\nPlease press ANY key for the next transaction");
            Console.ReadKey();
            Console.Clear();

        }//end function

        //prompt function that accepts a message that is printed to the screen then returns the input response
        static string Prompt(string msg){
            Console.Write(msg + ": ");
            return Console.ReadLine();
        }//end function

        //initialize_till function that accepts the count of every possible domination bill that is stored in the kiosk
        //then with each of those counts entries are created in the global dictionary till for every dominanation
        //the value, or count, of each of those different denomination entries is set as the input int in its respective variable
        static bool Initialize_Till(int hundred, int fiftyb, int twenty, int ten, int five, int two, int oneb,
                                    int onec, int fiftyc, int quater, int dime, int nickel, int penny){
            till.Add("0.01", penny);
            till.Add("0.05", nickel);
            till.Add("0.10", dime);
            till.Add("0.25", quater);
            till.Add("0.50", fiftyc);
            till.Add("1.00c", onec);
            till.Add("1.00", oneb);
            till.Add("2.00", two);
            till.Add("5.00", five);
            till.Add("10.00", ten);
            till.Add("20.00", twenty);
            till.Add("50.00", fiftyb);
            till.Add("100.00", hundred);
            return true;
        }//end function

        //cashOrCard function that has no input and returns a bool
        static bool cashOrCard(){
            //displays the options to pay with cash or card
            Console.WriteLine("0. Pay with cash\n1. Pay with Card");

            //use prompt function to ask user to choose cash or card
            string input = Prompt("Please choose an option");

            //if the user enters 0 then the function will return true for the user wanting to use cash
            if (input == "0"){
                return true;
            //else the user will be using card so the funciton returns false
            //there are limitations here because we are only checking if the user entered 0 signifying chash
            //there is no check for card, so the user could enter anything other than 0 and they will get the card option
            }else{
            return false;
            }
        }//end function

        //enterCash function that accepts the decimal total price and an empty queue
        //then the function returns the changeDue as a decimal
        static decimal enterCash(decimal total, Queue<string> cash_entered){
            //initializing vaiables
            decimal changeDue = 0;
            decimal tender = 0;
            decimal remaining = 0;
            Console.WriteLine();
            //this is a payment loop, while the tender entered is below the total then it will continue looping
            while (tender < total){
                //accepts an input of any denomination from the user as payment
                int count = 1;
                string input = decimal.Parse(Prompt("Payment " + count)).ToString("F");
                //adds that payment to the tender variable, adds the bill to the cash_entered queue, adds the bill to the till count,
                //and updates the remaining total to display to the user
                tender += decimal.Parse(input);
                cash_entered.Enqueue(input);
                till[input]++;
                remaining = total - tender;
                if (remaining > 0){
                    Console.WriteLine("Remaining  " + remaining.ToString("F"));
                }
            }//end while when tender entered is greater than transaction total
            //calculate change due based on the tender entered, then display the changeDue on the screen
            changeDue = tender - total;
            Console.WriteLine("\nChange    $" + changeDue);
            //return changeDue
            return changeDue;
        }//end function


        //changeBreaker function that accepts the decimal changeDue and an empty queue cash_entered and returns a bool
        static bool changeBreaker(decimal changeDue, Queue<string> cash_returned){
            
            //while the changeDue is greater than 0 then this if else statment will continue looping
            while (changeDue > 0.00m){
                //this is a greedy algorithm that checks for the highest possible denomination that can be returned first
                //once it is found that amount is subtracted from the changeDue, a bill of the greatest denomination found is taken
                //out of the till then a record of the bill is added to the cash_returned queue
                //the order is $100, $50, $20, $10, $5, $2, $1, 50¢, 25¢, 10¢, 5¢, 1¢
                //once the algorithm runs all the way through the change should be exactly $0.00 and the program
                //will return true for correctly broken change
                
                //since this is a till, the till is checked as well as the change due so no money is subtracted from the till that is not there
                //therefore there is an extra if else case at the end accounting for the inability to make change
                //if the kiosk does not have correct change the function will return false and the function breaks

                //$100
                if (changeDue >= 100.00m && till["100.00"] > 0){
                    till["100.00"]--;
                    cash_returned.Enqueue("100.00");
                    changeDue -= 100.00m;
                //$50
                }else if (changeDue >= 50.00m && till["50.00"] > 0){
                    till["50.00"]--;
                    cash_returned.Enqueue("50.00");
                    changeDue -= 50.00m;
                //$20
                }else if (changeDue >= 20.00m && till["20.00"] > 0){
                    till["20.00"]--;
                    cash_returned.Enqueue("20.00");
                    changeDue -= 20.00m;
                //$10
                }else if (changeDue >= 10.00m && till["10.00"] > 0){
                    till["10.00"]--;
                    cash_returned.Enqueue("10.00");
                    changeDue -= 10.00m;
                //$5
                }else if (changeDue >= 5.00m && till["5.00"] > 0){
                    till["5.00"]--;
                    cash_returned.Enqueue("5.00");
                    changeDue -= 5.00m;
                //$2
                }else if ((changeDue >= 2.00m && till["1.00"] < 10) && till["2.00"] > 0){
                    till["2.00"]--;
                    cash_returned.Enqueue("2.00");
                    changeDue -= 2.00m;
                //$1
                }else if (changeDue >= 1.00m && till["1.00"] > 0){
                    till["1.00"]--;
                    cash_returned.Enqueue("1.00");
                    changeDue -= 1.00m;
                //50¢
                }else if ((changeDue >= 0.50m && till["0.25"] < 6) && till["0.50"] > 0){
                    till["0.50"]--;
                    cash_returned.Enqueue("0.50");
                    changeDue -= 0.50m;
                //25¢
                }else if (changeDue >= 0.25m && till["0.25"] > 0){
                    till["0.25"]--;
                    cash_returned.Enqueue("0.25");
                    changeDue -= 0.25m;
                //10¢
                }else if (changeDue >= 0.10m && till["0.10"] > 0){
                    till["0.10"]--;
                    cash_returned.Enqueue("0.10");
                    changeDue -= 0.10m;
                //5¢
                }else if (changeDue >= 0.05m && till["0.05"] > 0){
                    till["0.05"]--;
                    cash_returned.Enqueue("0.05");
                    changeDue -= 0.05m;
                //1¢
                }else if (changeDue >= 0.01m && till["0.01"] > 0){
                    till["0.01"]--;
                    cash_returned.Enqueue("0.01");
                    changeDue -= 0.01m;
                //kiosk does not have enough change to return
                }else if (changeDue >= 0.01m){
                    return false;
                }
            }
            //correct change
            return true;
        }//end function
            
        //function that accepts the full cash_returned queue in order to dispense the change to the customer(to the screen)
        static void changeDispenser(Queue<string> cash_returned){
            //creating a string array to more easily print the bills to the screen since they are all in the same format
            string[] bills = {"100.00", "50.00", "20.00", "10.00", "5.00", "2.00", "1.00"};
            //while there are still items left in the cash_returned queue the loop will continue
            while (cash_returned.Count > 0){
                //get the first entry in the queue and store it to temp
                string temp = cash_returned.Dequeue();
                //if it is a bill the dequeued bill will be printed to the screen as returned
                //$100, $50, $20, $10, $5, $2, $1
                if (bills.Contains(temp)){
                    Console.WriteLine("${0} returned", Math.Truncate(double.Parse(temp)).ToString());
                //else if it is a coin the output is formatted individually
                //100¢
                }else if (temp == "1.00c"){
                    Console.WriteLine("100¢ returned");
                //50¢
                }else if (temp == "0.50"){
                    Console.WriteLine("5¢ returned");
                //25¢
                }else if (temp == "0.25"){
                    Console.WriteLine("25¢ returned");
                //10¢
                }else if (temp == "0.10"){
                    Console.WriteLine("10¢ returned");
                //5¢
                }else if (temp == "0.05"){
                    Console.WriteLine("5¢ returned");
                //1¢
                }else if (temp == "0.01"){
                    Console.WriteLine("1¢ returned");
                }
            }
            
        }//end function

        //tenderReturn function that covers the case that there is not enough change in the kiosk to break the changeDue
        //function takes both the full queues cash_inserted and cash_returned
        //function loops through each of the queues and undoes the adding or returning of bills in the kiosk
        //sets till back to before transaction
        static void tenderReturn(Queue<string> cash_inserted, Queue<string> cash_returned){
            while (cash_inserted.Count > 0){
                till[cash_inserted.Dequeue()]--;
            }
            while (cash_returned.Count > 0){
                till[cash_returned.Dequeue()]++;
            }
        }//end function

        //queueClear function that accepts two string queues and dequeues them until they are empty
        static void queueClear(Queue<string> cash_inserted, Queue<string> cash_returned){
            while (cash_inserted.Count > 0){
                cash_inserted.Dequeue();
            }
            while (cash_returned.Count > 0){
                cash_returned.Dequeue();
            }
        }//end function


        //cardReader function that accepts a decimal total price and returns a string array with the
        //card number and the price paid so far
        static string[] cardReader(decimal total){
            //creates a cash returned queue to handle cash back
            Queue<string> cash_returned = new Queue<string>();

            //initializing cashback, input, and returned string array variables
            string[] outcome = {"","","", ""};
            bool cashBack = false;
            string input = "";
            decimal moneyBack = 0.00m;

            //getting the card number
            outcome[0] = Prompt("Please enter the card number");
            //this is a test case so i do not have to re-enter card numbers
            //outcome[0] = "4111111111111111";

            //send entered card number to card number validator for validation
            //function returns the vendor or an error message
             string vendor = cardNumberValidator(outcome[0]);

            //if the return is not an error message then the card number is valid and the transaction progresses
            if (vendor != "No vendor identified" && vendor != "Invalid card number"){
                //prompting user for cash back
                input = Prompt("\nWould you like cash back?\nPlease enter 'y' or 'n'").ToLower();
                Console.WriteLine();
                //if the user wants cash back
                if (input.Contains("y")){
                    //ask user to enter how much cash back they want then send amount they want to changeBreaker
                    //changeBreaker checks if the kiosk can support the cash back changeBreaker returns a bool
                    input = decimal.Parse(Prompt("Please enter the amount of cashback you want")).ToString("F");
                    cashBack = changeBreaker(decimal.Parse(input), cash_returned);
                    moneyBack = decimal.Parse(input);
                    //if the kiosk can support the cash back
                    if (cashBack){
                        //the ammount entered is added to the total
                        total += decimal.Parse(input);
                        

                    //else the kiosk cannot support the cash back and a message is displayed and the old total is reprinted
                    }else{
                        Console.WriteLine("\nThis kiosk does not have enought cash to support this cash back request.");
                        Console.WriteLine("Total payment:  $" + total);
                        Console.WriteLine();
                    }
                }
                //send the validated card number and the updated total to the bank to validate the transaction
                //string array is returned - {card_number, remaining_total}
                outcome = MoneyRequest(outcome[0], total);
                outcome[2] = vendor;
            //else an error message was returned and the card_number was invalid
            }else {
                Console.WriteLine("\nError! {0}!!", vendor);
                outcome[1] = "";
                outcome[2] = "N/A";
            }
            //if there was cashback added to the total and the transactioon did not pass or was declined
            if (cashBack && ( outcome[1] == "declined" || decimal.Parse(outcome[1]) != total)){
                //the cashback is subtracted from the total and a message is displayed
                total -= decimal.Parse(input);
                moneyBack = 0.00m;
                Console.WriteLine("Cash back failed.\n");
                if (decimal.Parse(outcome[1]) > total){
                    outcome[1] = total.ToString();
                }
            //else if there was cashback added to the total and the transaction passed
            }else if (cashBack && decimal.Parse(outcome[1]) == total){
                //dispense the cashback to the customer
                changeDispenser(cash_returned);
            }
            outcome[3] = moneyBack.ToString();
            //return outcome string array
            return outcome;
        }//end function

        //cardNumberValidator function that accepts a card_number as a string and returns a string
        //the return string is either the vendor of a validated card number or an error message
        static string cardNumberValidator(string card_number){
            //initializing return string, sum variable, and a string array where individual card numbers are stored
            string vendor = "";
            int sum = 0;
            string[] numbers = new string[card_number.Length];
            //for loop that adds each card number to its own array index
            for (int i = 0; i < numbers.Length; i++){
                numbers[i] = card_number[i].ToString();
            }
            //switch case that checks for the card vendor, if no vendor is identified error message is returned
            switch (card_number.First())
            {
                case '2': {vendor = "Mastercard"; break;}
                case '3': {vendor = "American Express"; break;}
                case '4': {vendor = "Visa"; break;}
                case '5': {vendor = "Mastercard"; break;}
                case '6': {vendor = "Discover"; break;}
                default: {return "No vendor identified";}
            }
            //code that creates the Luhn algorithm
            //requires two for loops, starting at the second digit from the right in the card number double every other digit
            //if one of those digits are double then add the two digits together
            //then add all the numbers in the string
            //if that number is divisible by 10 then the number is invalid
            for (int i = (card_number.Length - 2); i >= 0; i-= 2){
                numbers[i] = ((Convert.ToInt32(numbers[i]))*2).ToString();
                if (numbers[i].Length > 1){
                    numbers[i] = ((int.Parse(numbers[i][0].ToString())) + (int.Parse(numbers[i][1].ToString()))).ToString();
                }
            }
            for (int i = 0; i < numbers.Length; i++){
                sum += (Convert.ToInt32(numbers[i]));
            }
            if (sum % 10 == 0){
                //this means the card is valid so the vendor is returned
                return vendor;
            }
            //this means the card number is invalid and an error message is returned
            return "Invalid card number";
        }//end function
        
        //create header function that accepts the transaction number then formats the number into
        //a string with the date and time with the month as a three letter abbreviation
        static string createHeader(int transactionNumber){
            string header = transactionNumber.ToString().PadLeft(4, '0') + " ";
            string date = "";
            string time = "";
            switch (DateTime.Today.Month)
            {
                case 1: {date = "Jan-"; break;}
                case 2: {date = "Feb-"; break;}
                case 3: {date = "Mar-"; break;}
                case 4: {date = "Apr-"; break;}
                case 5: {date = "May-"; break;}
                case 6: {date = "Jun-"; break;}
                case 7: {date = "Jul-"; break;}
                case 8: {date = "Aug-"; break;}
                case 9: {date = "Sep-"; break;}
                case 10: {date = "Oct-"; break;}
                case 11: {date = "Nov-"; break;}
                case 12: {date = "Dec-"; break;}
                default:
                    break;
            } 
            //using datetime object to get the rest of the date and time in the header
            //date += DateTime.Today.Day.ToString() + "-" + DateTime.Today.Year.ToString() + " ";
            date += DateTime.Today.ToString("dd-yyyy ");
            time = DateTime.Now.ToString("hh:mm:sstt ");
            //creating a full header string to return
            header += date + time ;
            //return header
            return header;
        }//end function


        //bank request
        static string[] MoneyRequest(string account_number, decimal amount){
            bool pass = rnd.Next(100) < 50;
            bool declined = rnd.Next(100) < 50;
            if (pass)   {
                return new string[] {account_number, amount.ToString(), "", ""};
            }else{
                if (!declined){
                    return new string[] {account_number, (amount / rnd.Next(2,6)).ToString(), "", ""};
                }else{
                    return new string[] {account_number, "declined", "", ""};
                }
            }
        }
    }
}
        //A dictionary will be created to link each denomination to a count value
        //This dictionary will then be initialized with a realistic large number of "bills"
            //A dictionary with a string key and an int value "till" initialized with a set
            //number of each denomination, the start up cash
            //Till initialization process sent to tillInitializer function


         //This program will have a master loop that keeps it open so the money count will not reset
            //Create a bool variable "loop" that is initialized to true
            //Open a while loop with loop as the condition so the program will be on an endless loop


        //Next, the user will be prompted to input the prices of their items that will be stored in a list
        //The input will be checked and validated after each price entry
            //The validated "input" will then be parsed to a double and add to "total"


        //The user will break out of the program once they are done by inputting no price
            //The while loop will then restart and continue allowing the user to input items until they enter ""


        //The total is then calculated and displayed to the user who is then asked to provide any denomination
        //bill or coin until the total is met/exceeded
            //Total displayed to screen for user
            //Create while loop with the condition double "tender" is less than "total"
            //Add bills the user enters to "tender" and update the "till" values of each bill inserted


        //If a $1 bill is inserted then the value of the $1 key is increased by 1 and so on for all denominations
        //This will be a switch case to operate upon the proper key in the dictionary based on the users input
            //Add "bill" the user enters to "tender" to calculate total payment inserted
            //Using a switch case checking validated "bill" agaisnt possible "till" keys
            //Update the "till" values of each bill inserted then recheck the while "tender" is less than "total" condition


        //From the total price and the total tender the change will be calculated
        //Once the change is calculated it will be written to the screen, then the program will break the total change
        //into the greatest possible denominations increments of change and the returned bills will be written to console
            //Once "tender" is greater than "total" the double "changeDue" will be calculated
            //"changeDue" will be passed through an if-if else statement nested within a while loop that will make sure the highest possible
            //denomination change is returned to the customer
            //while "changeDue" is greater than 0 the program will check for the greates possible denomination from "chageDue"
            //then continue subtracting that bill until "changeDue" is less than the bill value, then move on to the next smaller
            //denomination of bill or coin
            //if the change cannot be made the kiosk will offer a different payment method


        //As the change is returned the count value for each of the bills returned will decrease
            //The "changeBreaker" function will store the bills returned then once the correct change is made the
            //difference in bills will be returned and "till" will be updated
