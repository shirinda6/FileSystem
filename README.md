# FileSystem

![fileSys](https://user-images.githubusercontent.com/81624047/196695607-ea2b9972-b682-4577-b8ab-5c6bd31fe1d2.png)

#### Above we can see a descriptive diagram of our file system. </br>
The dat file itself, contain a system header, beginning with the prefix to show that it is a BGUFS file type. </br>
That header also contains the number of headers we have, and the amount of content in that saved file  </br>(There can be multiple links to the same content, so those numbers are not necessarily equal). </br> </br>
#### This is based on the concept guidelines from the file system as seen in the picture below:

![p2](https://user-images.githubusercontent.com/81624047/196696029-93235ee9-0aa5-4153-9e50-bcd4858d495c.png)

#### The headers, as seen in the diagram: </br>

![p3](https://user-images.githubusercontent.com/81624047/196696126-e234c218-c759-4194-811a-9383f4103727.png)

#### Are based on the concept guidelines from the dir function, where the printed output is in the following format: </br>
![p4](https://user-images.githubusercontent.com/81624047/196696183-e31bcb7e-b4d1-4845-8ce8-88c873434de2.png)


With an addition of a content index to show us in which place the content of the header is held in the content section (Which is hidden from the user in the print function because it is irrelevant).
In our dat file, we use “|” to separate the fields instead of “,” since “,” can appear as part of the name of the file (Windows allows you to use “,” in naming files, but not “|” hence it was chosen).
We can see an example of a test file below: </br>

![p5](https://user-images.githubusercontent.com/81624047/196696228-ef4f981f-328d-4023-bfa5-ca811f3c7c15.png)

#### Which is structured just like the diagram: </br>
![p6](https://user-images.githubusercontent.com/81624047/196696342-dff5c479-bcaf-4c8d-b9b4-fea4ce0d7a27.png)


Whenever we need to do a action on the file system, since you can’t write in the middle of a file, only add to the end or rewrite it. </br> We have to load the entire file into our system, using the 2 counts in the file header to know how many lines to read of headers and how many of content. </br>
##### We separate the headers and content into two separate arrays so we could manipulate them easily, just as show in the diagram: </br>

![p7](https://user-images.githubusercontent.com/81624047/196696481-01066186-bb41-468e-b624-a66a930d92d8.png)

After having the data in the system, we can do the various actions like adding, removing, making links or sorting, which results in different arrays of headers and content,  </br> 
##### which are then being wrote back into the file and overwrite the previous one, just as shown in the diagram:

![p8](https://user-images.githubusercontent.com/81624047/196696532-4e3c468e-1985-47cc-a3ed-e2caf3dfe477.png)

We make sure to load the files only if needed, we do not load the dat file if we need to create a new one. </br> We also only save if needed, we do not save the dat file if we did actions that did not change anything, as showing the hash value, extracting a file, or printing the dir. </br>
When removing content, an empty place is left in its place, which can be handled by optimizing the system or through various sorting operations, when we always make sure to properly update the regular headers when we make changes in the content and update the link headers when we make changes in the regular headers to ensure all headers always correctly point on their respective content. </br>
While regular headers index points to the place of the content in the content array, we use the header index for the links as an index to the place of the header that the link is referring in the header array,  instead of making a separate value field for content/reference we use the same spot for multiple purposes.
