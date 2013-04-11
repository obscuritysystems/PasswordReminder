using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Data;

namespace PasswordReminder
{
    class ConfigFileReader
    {

        private Dictionary<String,String> configOptions;        
        private String fileLocation;

        public Boolean isConfigurationLoaded = false;

        public ConfigFileReader(String fileLocationIn)
        {
            configOptions = new Dictionary<string,string>();
            fileLocation = fileLocationIn;
            LoadConfig();

        }

        public int numberOfOptions()
        {
            return configOptions.Count;
        }

        public string getConfigOption(String Option)
        {            
            try 
			{ 
				String value = configOptions[Option].Trim();
				return value;
			}
			catch(Exception ex)
			{
				Console.WriteLine("Could Not Find Value for Option:" +Option);
				Console.WriteLine(".....Please Define a value...... ");
				Console.WriteLine("Example: OPTION=VALUE" );
				Console.WriteLine("Example: "+Option+"=VALUE" );
				return "";
			}
			
        }


        /*
         */
        public void printConfigOptions()
        {
            foreach (KeyValuePair<string, string> dictionaryElement in configOptions)
            {                
                Console.WriteLine("Key: " + dictionaryElement.Key + " = " + dictionaryElement.Value);                
            }        
        }
        
        /*
         */
        private void LoadConfig()
        {

            //if (FileExists(fileLocation))
            //{
                // Open config file for config information then load information memory 
                String text;
                TextReader tr = new StreamReader(fileLocation);
                do
                {
                    String[] splitConfigOptions;
                    String[] commentSplit;
                    text = tr.ReadLine();

                    // if text we are getting from the file is not null
                    if (text != null)
                    {
                        if (text.Contains("#"))
                        {
                            commentSplit = text.Split('#');
                            text = commentSplit[0];

                        }

                        splitConfigOptions = text.Split('=');

                        if (splitConfigOptions.Length == 2)
                        {
                            configOptions.Add(splitConfigOptions[0].ToString().Trim(), splitConfigOptions[1].ToString().Trim());
                        }
                    }

                } while (text != null); // null must mean end of file                                           
                tr.Close();
                isConfigurationLoaded = true;
            //}
            //else
            //{
            //    isConfigurationLoaded = false;
            //}
        }// end function


        //-----------------------------------------------------------
        // FUNCTION: DirExists
        // Determines whether the specified directory name exists.
        // IN: [sDirName] - name of directory to check for
        // Returns: True if the directory exists, False otherwise
        //-----------------------------------------------------------
        private static bool DirExists(string sDirName)
        {
            try
            {
                return (System.IO.Directory.Exists(sDirName));    //Check for file
            }
            catch (Exception)
            {
                return (false);                                 //Exception occured, return False
            }
        }

        //-----------------------------------------------------------
        // FUNCTION: FileExists
        // Determines whether the specified file exists
        // IN: [sPathName] - file to check for
        // Returns: True if file exists, False otherwise
        //-----------------------------------------------------------
        private static bool FileExists(string sPathName)
        {
            try
            {
                return (System.IO.Directory.Exists(sPathName));  //Exception for folder
            }
            catch (Exception)
            {
                return (false);                                   //Error occured, return False
            }
        }


     }// end class
}// end namespace
