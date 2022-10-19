using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGUFS
{
    class Program
    {
        #region Initilize variables
        public static ArrayList headers = new ArrayList();
        public static ArrayList contents = new ArrayList();
        public static int headerCount = 0;
        public static int contentCount = 0;
        #endregion


        //Auxiliary function
        private static int getFileHeaderIndex(string fileName)
        {
            for (int i = 0; i < headers.Count; i++)
            {
                //Check if empty line
                if (headers[i].ToString().Equals(""))
                {
                    continue;
                }

                //Get the header meta data and check it
                string[] metaData = headers[i].ToString().Split('|');
                if (metaData[0].ToString().Equals(fileName))
                    return i;
            }
            return -1;
        }

        static void Main(string[] args)
        {
            #region Loading arguments
            //Load the arguments
            String oper = args[0];
            String fileSystem = args[1];
            String fileName = "";
            String newFileName = "";
            //Load additional arguments if needed
            if (args.Length >= 3)
                fileName = args[2];
            if (args.Length >= 4)
                newFileName = args[3];
            #endregion

            #region File system creation
            //Check if the filesystem is for creation
            if (oper.Equals("-create"))
            {
                try
                {
                    FileStream fileStream = File.Create(fileSystem);
                    string datFileHeader = "BGUFS_|" + headerCount + "|" + contentCount + "\n";

                    byte[] configBytes = Encoding.ASCII.GetBytes(datFileHeader);

                    fileStream.Close();

                    File.WriteAllBytes(fileSystem, configBytes);
                }
                catch (Exception)
                {
                    Console.WriteLine(fileSystem + " filesystem creation has failed.");
                }
                Environment.Exit(0);
            }
            #endregion

            #region Loading the dat file into the data base
            //If the filesystem is not for creation, for but function
            //We need to load up the data base so we could change it
            StreamReader datFileReader = new StreamReader(fileSystem);
            string fileHeader = datFileReader.ReadLine();
            string[] fileMetaData = fileHeader.Split("|");
            if (fileMetaData[0] != "BGUFS_")
            {
                Console.WriteLine("Not a BGUFS file");
                Environment.Exit(0);
            }

            //Load the amount of headers and contents for reading
            headerCount = int.Parse(fileMetaData[1]);
            contentCount = int.Parse(fileMetaData[2]);

            //Read all the headers into the data base
            for (int i = 0; i < headerCount; i++)
            {
                string headerLine = datFileReader.ReadLine();
                headers.Add(headerLine);
            }

            //Read all the content into the data base
            for (int i = 0; i < contentCount; i++)
            {
                string contentLine = datFileReader.ReadLine();
                contents.Add(contentLine);
            }

            datFileReader.Close();
            #endregion

            //metaData[0] = name
            //metaData[1] = size
            //metaData[2] = date
            //metaData[3] = type
            //metaData[4] = index
            //metaData[5] = file name

            #region Extract a file from the system
            if (oper.Equals("-extract"))
            {
                //Get the index of the header to extract
                int headerIndexExtract;
                if ((headerIndexExtract = getFileHeaderIndex(fileName)) == -1)
                {
                    Console.WriteLine("file does not exist");
                    Environment.Exit(0);
                }

                //Get the content of the file to extract
                string[] metaDataExtract = headers[headerIndexExtract].ToString().Split('|');
                int contentIndexExtract = int.Parse(metaDataExtract[4]);
                Byte[] bytes = Convert.FromBase64String(contents[contentIndexExtract].ToString());

                //Write the content into the extracted file destination
                File.WriteAllBytes(args[2], bytes);

                Environment.Exit(0);
            }
            #endregion

            #region Dir - show all files in the system
            if (oper.Equals("-dir"))
            {
                //Go through all the headers and print them
                for (int i = 0; i < headers.Count; i++)
                {
                    //If the header is empty line skip it
                    if (headers[i].ToString() == "")
                    {
                        continue;
                    }
                    string[] metaDataDir = headers[i].ToString().Split('|');
                    //If header is link print the original name, otherwise just regualr details
                    if (metaDataDir.Equals("link"))
                        Console.WriteLine(metaDataDir[0] + "," + metaDataDir[1] + "," + metaDataDir[2] + "," + metaDataDir[3] + "," + metaDataDir[5]);
                    else
                        Console.WriteLine(metaDataDir[0] + "," + metaDataDir[1] + "," + metaDataDir[2] + "," + metaDataDir[3]);
                }

                Environment.Exit(0);
            }
            #endregion

            #region Hash - print the md5 hash value of a file
            if (oper.Equals("-hash"))
            {
                //Get the index of the header to hash
                int headerIndexHash;
                if ((headerIndexHash = getFileHeaderIndex(fileName)) == -1)
                {
                    Console.WriteLine("file does not exist");
                    Environment.Exit(0);
                }

                //If the header is a link, change the header into the header of the original
                string[] metaDataHash = headers[headerIndexHash].ToString().Split("|");
                if (metaDataHash[3] == "link")
                {
                    metaDataHash = headers[int.Parse(metaDataHash[4])].ToString().Split("|");
                }
                int contentIndexHash = int.Parse(metaDataHash[4]);

                //Start hashing into MD5
                using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
                {
                    //Hash the content
                    string contentString = contents[contentIndexHash].ToString();
                    byte[] contentBytes = Convert.FromBase64String(contentString);
                    byte[] contentBytesHashed = md5.ComputeHash(contentBytes);

                    //Convert the hashed content into a hexadecimal string for display
                    StringBuilder stringBuilder = new StringBuilder();
                    for (int i = 0; i < contentBytesHashed.Length; i++)
                        stringBuilder.Append(contentBytesHashed[i].ToString("X2"));

                    Console.WriteLine(stringBuilder.ToString());
                }
                Environment.Exit(0);
            }
            #endregion

            Boolean needToUpdateLinks = false;

            #region Activating the function
            //After loading the data base
            //Do different function based on the operator given
            switch (oper)
            {
                case "-add":
                    #region Add

                    //Get all the inforamtion of the file

                    FileInfo fileInfo = new FileInfo(fileName);
                    string name = fileInfo.Name;
                    long size = fileInfo.Length;
                    DateTime date = fileInfo.CreationTime;

                    //Check if file with such name already exist
                    if ((getFileHeaderIndex(name)) != -1)
                    {
                        Console.WriteLine("file already exist");
                        Environment.Exit(0);
                    }

                    //string header = filePath + "|" + size + "|" + dateTime + "|" + "regular" + "|" + contentCount;
                    //Complie the header for the file and add it
                    string[] metaDataTemp = { name, size.ToString(), date.ToString(), "regular", contentCount.ToString() };
                    string header = String.Join("|", metaDataTemp);
                    headers.Add(header);

                    //Complie the content for the file and add it
                    Byte[] dataBytes = File.ReadAllBytes(fileName);
                    string content = Convert.ToBase64String(dataBytes);
                    contents.Add(content);

                    //Update the header content count
                    headerCount++;
                    contentCount++;

                    #endregion
                    break;

                case "-remove":
                    #region Remove

                    //Get the index of the header to remove
                    int headerIndex;
                    if ((headerIndex = getFileHeaderIndex(fileName)) == -1)
                    {
                        Console.WriteLine("file does not exist");
                        Environment.Exit(0);
                    }

                    string[] metaDataRemove = headers[headerIndex].ToString().Split('|');

                    //If the header is regular
                    if (metaDataRemove[3] == "regular")
                    {
                        //get the content index and remove it
                        int contentIndex = 0;
                        contentIndex = int.Parse(metaDataRemove[4]);
                        contents[contentIndex] = "";
                        //Check and remove any links to this header
                        for (int i = 0; i < headers.Count; i++)
                        {
                            //Check if empty line
                            if (headers[i].ToString() == "")
                            {
                                continue;
                            }

                            metaDataRemove = headers[i].ToString().Split('|');
                            if (metaDataRemove[3] == "link")
                            {
                                //If the current header is a link to our original header, remove it
                                if (int.Parse(metaDataRemove[4]) == headerIndex)
                                    headers[i] = "";
                            }
                        }
                    }

                    //Remove the orignal header
                    headers[headerIndex] = "";

                    #endregion
                    break;

                case "-rename":
                    #region Rename

                    //Get the index of the header to rename
                    int headerIndexRename;
                    if ((headerIndexRename = getFileHeaderIndex(fileName)) == -1)
                    {
                        Console.WriteLine("file does not exist");
                        Environment.Exit(0);
                    }

                    //Check if the name is taken
                    if (getFileHeaderIndex(newFileName) != -1)
                    {
                        Console.WriteLine("file already exist");
                        Environment.Exit(0);
                    }

                    //Rename the orignal header
                    string[] metaDataRename = headers[headerIndexRename].ToString().Split('|');
                    metaDataRename[0] = newFileName;
                    headers[headerIndexRename] = string.Join("|", metaDataRename);

                    //Update the new name in all the links to this header
                    for (int i = 0; i < headers.Count; i++)
                    {
                        //Check if empty line
                        if (headers[i].ToString() == "")
                        {
                            continue;
                        }
                        metaDataRename = headers[i].ToString().Split('|');
                        if (metaDataRename[3] == "link")
                        {
                            if (metaDataRename[5] == fileName)
                            {
                                metaDataRename[5] = newFileName;
                                headers[i] = string.Join("|", metaDataRename);
                            }
                        }
                    }
                    #endregion
                    break;

                case "-optimize":
                    #region Optimize

                    int counter;
                    //Optimize the headers
                    for (int i = 0; i < headers.Count; i++)
                    {
                        counter = 1;
                        if (!headers[i].Equals("")) // If header found
                        {
                            while (i - counter >= 0 && headers[i - counter].Equals(""))//Try moving it to the left until you can't
                            {
                                //Bubble sort your way to the left
                                headers[i - counter] = headers[i - counter + 1];
                                headers[i - counter + 1] = "";
                                counter++;
                            }
                        }
                    }
                    //Remove all the empty spaces from the end
                    int index = headers.Count - 1;
                    while (headers[index].Equals("") && index >= 0)
                    {
                        headers.RemoveAt(index);
                        index--;
                    }

                    //Optimize the content
                    string[] metaData = null;
                    int headerIndexOptimize = 0;
                    for (int i = 1; i < contents.Count; i++)
                    {
                        counter = 1;
                        if (!contents[i].Equals("")) // If content found
                        {
                            if (contents[i - 1].Equals(""))//If we need to move content, pin point it's header
                            {
                                for (int j = 0; j < headers.Count; j++)
                                {
                                    //Check if empty line
                                    if (headers[i].ToString() == "")
                                    {
                                        continue;
                                    }

                                    //Extract the file name from the string
                                    metaData = ((String)headers[j]).Split("|");
                                    //If its a link the content index is not relevent
                                    if (metaData[3].Equals("link"))
                                    {
                                        continue;
                                    }
                                    string contentIndex = metaData[4];
                                    if (int.Parse(contentIndex) == i)
                                    {
                                        //Save the current index to update later
                                        headerIndex = j;
                                        break;
                                    }
                                }
                                //Start moving the content left
                                while (i - counter >=0 && contents[i - counter].Equals(""))//Try moving it to the left until you can't
                                {
                                    //Bubble sort your way to the left
                                    contents[i - counter] = contents[i - counter + 1];
                                    contents[i - counter + 1] = "";
                                    counter++;
                                }
                                //Update the header with the new location
                                metaData[4] = (i - counter + 1).ToString();
                                headers[headerIndexOptimize] = string.Join("|", metaData);
                            }
                        }
                    }
                    //Remove all the empty spaces from the end
                    index = contents.Count - 1;
                    while (contents[index].Equals("") && index >= 0)
                    {
                        contents.RemoveAt(index);
                        index--;
                    }
                    needToUpdateLinks = true;

                #endregion
                    break;

                case "-sortAB":
                    #region SortAB

                    headers.Sort(new sorterAB());

                    needToUpdateLinks = true;

                    #endregion
                    break;

                case "-sortDate":
                    #region SortDate

                    headers.Sort(new sorterDate());

                    needToUpdateLinks = true;

                    #endregion
                    break;

                case "-sortSize":
                    #region SortSize

                    headers.Sort(new sorterSize());

                    needToUpdateLinks = true;

                    #endregion
                    break;

                case "-addLink":
                    #region AddLink

                    int indexRegualr;
                    //Check if the regualr file exists in the system
                    if ((indexRegualr = getFileHeaderIndex(args[3])) == -1)
                    {
                        Console.WriteLine("file does not exist");
                        
                        Environment.Exit(0);
                    }

                    //Check if the link name is not taken
                    if (getFileHeaderIndex(args[2]) != -1)
                    {
                        Console.WriteLine("file already exist");
                        Environment.Exit(0);
                    }

                    //Generate the link header based on arguments and the orignal regular header
                    List<string> metaDataLink = headers[indexRegualr].ToString().Split('|').ToList();
                    metaDataLink.Add(metaDataLink[0]);
                    metaDataLink[0] = args[3];
                    metaDataLink[3] = "link";
                    metaDataLink[4] = indexRegualr.ToString();
                    string linkHeader = string.Join("|", metaDataLink);

                    //Add the new link header to the headers
                    headers.Add(linkHeader);
                    headerCount++;

                    #endregion
                    break;

                default:
                    Console.WriteLine("Command not supported.");
                    break;
            }
            #endregion

            #region Update link header location if header place changed

            if (needToUpdateLinks)
            {
                //Search list for links
                for (int i = 0; i < headers.Count; i++)
                {
                    // If Empty line, skip
                    if (headers[i].ToString().Equals(""))
                    {
                        continue;
                    }
                    string[] metaDataLink = (headers[i]).ToString().Split("|"); //get the metadata strings

                    //Check if link file
                    if (metaDataLink[3].Equals("link"))
                    {
                        // Search the list for the file
                        for (int j = 0; j < headers.Count; j++)
                        {
                            // If Empty line, skip
                            if (((String)headers[i]).Equals("") || (String)headers[i] == null)
                            {
                                continue;
                            }

                            //get the metadata strings
                            string[] metaDataFile = ((String)headers[i]).Split("|");

                            // if the link links to this file
                            if (metaDataFile[0].Equals(metaDataLink[5]))
                            {
                                // update the link file index to current index
                                metaDataLink[4] = j.ToString();
                                headers[i] = metaDataLink[0] + "|" + metaDataLink[1] + "|" + metaDataLink[2] + "|" + metaDataLink[3] + "|" + metaDataLink[4];
                                //continue to the next link file
                                break;
                            }
                        }
                    }
                }
            }

            #endregion


            #region Saving the data base into the dat file
            //After we are done with the function, write the new data base to the file
            //Write the dat file header
            string fileData = "BGUFS_" + "|" + headerCount.ToString() + "|" + contentCount.ToString() + "\n";

            //Write the headers into the dat file
            for (int i = 0; i < headers.Count; i++)
                fileData += headers[i].ToString() + "\n";

            //Write the content into the dat file
            for (int i = 0; i < contents.Count; i++)
                fileData += contents[i].ToString() + "\n";

            //Write all the file data into the dat file
            byte[] fileDataBytes = Encoding.ASCII.GetBytes(fileData);
            File.WriteAllBytes(fileSystem, fileDataBytes);
            #endregion

        }

    }

    //Sorter classes for custom sorting of the headers
    class sorterAB : IComparer
    {
        int IComparer.Compare(Object header1, Object header2)
        {
            if (header1.ToString() == "" && header2.ToString() != "") return 1;
            if (header1.ToString() != "" && header2.ToString() == "") return -1;

            //Extract the file name from the string
            string[] metaData1 = (header1).ToString().Split("|");
            string heaer1Name = metaData1[0];

            string[] metaData2 = (header2).ToString().Split("|");
            string heaer2Name = metaData2[0];

            // Use CaseInsensitiveComparer to automatically compare by abc order
            return ((new CaseInsensitiveComparer()).Compare(heaer1Name, heaer1Name));
        }
    }

    class sorterSize : IComparer
    {
        int IComparer.Compare(Object header1, Object header2)
        {
            //Take care of empty line cases
            if (header1.ToString() == "" && header2.ToString() != "") return 1;
            if (header1.ToString() != "" && header2.ToString() == "") return -1;

            //Extract the file name from the string
            string[] metaDataFirst = (header1).ToString().Split("|");
            int header1Size = int.Parse(metaDataFirst[1].ToString());

            string[] metaDataSecond = (header2).ToString().Split("|");
            int header2Size = int.Parse(metaDataSecond[1].ToString());

            // Use CaseInsensitiveComparer to automatically compare by abc order
            if (header1Size.CompareTo(header2Size) != 0)
            {
                return header1Size.CompareTo(header2Size);
            }
            else
            {
                string header1Name = metaDataFirst[0];
                string header2Name = metaDataSecond[0];
                return ((new CaseInsensitiveComparer()).Compare(header1Name, header2Name));
            }
        }
    }

    class sorterDate : IComparer
    {
        int IComparer.Compare(Object header1, Object header2)
        {
            //Take care of empty line cases
            if (header1.ToString() == "" && header2.ToString() != "") return 1;
            if (header1.ToString() != "" && header2.ToString() == "") return -1;

            //Extract the dates from the string
            string[] metaData1 = (header1).ToString().Split("|"); //get the metadata strings
            DateTime header1Date = DateTime.Parse(metaData1[2].ToString());

            string[] metaData2 = (header2).ToString().Split("|");
            DateTime header2Date = DateTime.Parse(metaData2[2].ToString());


            //Use date time built in compare to compare the files by their date
            // Use CaseInsensitiveComparer to automatically compare by abc order
            if (header1Date.CompareTo(header2Date) != 0)
            {
                return header1Date.CompareTo(header2Date);
            }
            else
            {
                string heaer1Name = metaData1[0];
                string heaer2Name = metaData2[0];
                return ((new CaseInsensitiveComparer()).Compare(heaer1Name, heaer2Name));
            }
        }
    }
}
