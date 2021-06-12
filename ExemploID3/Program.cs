using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExemploID3
{
    class Program
    {
    /// <summary>
    /// Class to represent an attribute used in the decision to class
    /// </summary>
    public class Attribute
    {
        ArrayList mValues;
        string mName;
        object mLabel;

        /// <summary>
        /// Initializes a new instance of a class Atribute
        /// </summary>
        /// <param name="name">Indicates the attribute name</param>
        /// <param name="values">Indicates the possible values for the attribute</param>
        public Attribute(string name, string[] values)
        {
            mName = name;
            mValues = new ArrayList(values);
            mValues.Sort();
        }

        public Attribute(object Label)
        {
            mLabel = Label;
            mName = string.Empty;
            mValues = null;
        }

        /// <summary>
        /// Indicates the attribute name
        /// </summary>
        public string AttributeName
        {
            get
            {
                return mName;
            }
        }

        /// <summary>
        /// Returns an array with the attribute values
        /// </summary>
        public string[] values
        {
            get
            {
                if (mValues != null)
                    return (string[])mValues.ToArray(typeof(string));
                else
                    return null;
            }
        }

        /// <summary>
        /// Indicates whether a value is allowed for this attribute
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool isValidValue(string value)
        {
            return indexValue(value) >= 0;
        }

        /// <summary>
        /// Returns the index of a value
        /// </summary>
        /// <param name="value">Value to be returned</param>
        /// <returns>The value of the index in which the value of the position is</returns>
        public int indexValue(string value)
        {
            if (mValues != null)
                return mValues.BinarySearch(value);
            else
                return -1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (mName != string.Empty)
            {
                return mName;
            }
            else
            {
                return mLabel.ToString();
            }
        }
    }

    /// <summary>
    /// Class representing the tree of mounted decision;
    /// </summary>
    public class TreeNode
    {
        private ArrayList mChilds = null;
        private Attribute mAttribute;

        /// <summary>
        /// Initializes a new instance of the TreeNode
        /// </summary>
        /// <param name="attribute">Attribute for which the node is connected</param>
        public TreeNode(Attribute attribute)
        {
            if (attribute.values != null)
            {
                mChilds = new ArrayList(attribute.values.Length);
                for (int i = 0; i < attribute.values.Length; i++)
                    mChilds.Add(null);
            }
            else
            {
                mChilds = new ArrayList(1);
                mChilds.Add(null);
            }
            mAttribute = attribute;
        }

        /// <summary>
        /// Adds a child TreeNode this treenode the name of the branch indicicado ValueName
        /// </summary>
        /// <param name="treeNode">TreeNode child to be added</param>
        /// <param name="ValueName">Branch name where the TreeNode is created</param>
        public void AddTreeNode(TreeNode treeNode, string ValueName)
        {
            int index = mAttribute.indexValue(ValueName);
            mChilds[index] = treeNode;
        }

        /// <summary>
        /// Returns the total number of children of the node
        /// </summary>
        public int totalChilds
        {
            get
            {
                return mChilds.Count;
            }
        }

        /// <summary>
        /// Returns the child node of a node
        /// </summary>
        /// <param name="index">Indice do nَ filho</param>
        /// <returns>An object of the TreeNode class representing the node</returns>
        public TreeNode getChild(int index)
        {
            return (TreeNode)mChilds[index];
        }

        /// <summary>
        /// Attribute that is connected to Node
        /// </summary>
        public Attribute attribute
        {
            get
            {
                return mAttribute;
            }
        }

        /// <summary>
        /// Returns the child of a node by the branch name that leads up to it
        /// </summary>
        /// <param name="branchName">Branch name</param>
        /// <returns>O nَ</returns>
        public TreeNode getChildByBranchName(string branchName)
        {
            int index = mAttribute.indexValue(branchName);
            return (TreeNode)mChilds[index];
        }
    }

    /// <summary>
    /// Class that implements a decision tree using ID3 algorithm
    /// </summary>
    public class DecisionTreeID3
    {
        private DataTable mSamples;
        private int mTotalPositives = 0;
        private int mTotal = 0;
        private string mTargetAttribute = "result";
        private double mEntropySet = 0.0;

        /// <summary>
        /// Returns the total number of positive samples in a sample table
        /// </summary>
        /// <param name="samples">DataTable with samples</param>
        /// <returns>The total number of positive samples</returns>
        private int countTotalPositives(DataTable samples)
        {
            int result = 0;

            foreach (DataRow aRow in samples.Rows)
            {
                if ((bool)aRow[mTargetAttribute] == true)
                    result++;
            }

            return result;
        }

        /// <Summary>
        /// Calculates the entropy given the following formula
        /// + -p Log2p + - p-log2p-
        ///
        /// Wherein: p + is the proportion of positive values
        /// P is the proportion of negative values
        /// </ Summary>
        /// <Param name = "positives"> positive values count </ param>
        /// <Param name = "negatives"> negative values count </ param>
        /// <Returns> Returns the value of entropy </ returns>
        private double calcEntropy(int positives, int negatives)
        {
            int total = positives + negatives;
                double ratioPositive = 0;
                double ratioNegative = 0;
                if (total > 0)
                {
                    ratioPositive = (double)positives / total;
                    ratioNegative = (double)negatives / total;
                }

            if (ratioPositive != 0)
                ratioPositive = -(ratioPositive) * System.Math.Log(ratioPositive, 2);
            if (ratioNegative != 0)
                ratioNegative = -(ratioNegative) * System.Math.Log(ratioNegative, 2);

            double result = ratioPositive + ratioNegative;

            return result;
        }

        /// <Summary>
         /// Scans sample table checking an attribute and if the result is positive or negative
         /// </ Summary>
         /// <Param name = "sample"> DataTable with samples </ param>
         /// <Param name = "attribute"> Attribute to search </ param>
         /// <Param name = "value"> allowed value for the </ param>
         /// <Param name = "positives"> will contain the nro of all attributes with the value determined with a positive result </ param>
         /// <Param name = "negatives"> will contain the nro of all attributes with the value determined with negative results </ param>
        private void getValuesToAttribute(DataTable samples, Attribute attribute, string value, out int positives, out int negatives)
        {
            positives = 0;
            negatives = 0;

            foreach (DataRow aRow in samples.Rows)
            {
                if (((string)aRow[attribute.AttributeName] == value))
                    if ((bool)aRow[mTargetAttribute] == true)
                        positives++;
                    else
                        negatives++;
            }
        }

        /// <Summary>
        /// Estimated gain of an attribute
        /// </ Summary>
        /// <Param name = "attribute"> Attribute to be calculated </ param>
        /// <Returns> The gain of the </ returns>
        private double gain(DataTable samples, Attribute attribute)
        {
            string[] values = attribute.values;
            double sum = 0.0;

            for (int i = 0; i < values.Length; i++)
            {
                int positives, negatives;

                positives = negatives = 0;

                getValuesToAttribute(samples, attribute, values[i], out positives, out negatives);

                double entropy = calcEntropy(positives, negatives);
                    //if(entropy !=null)
                sum += -(double)(positives + negatives) / mTotal * entropy;
            }
            return mEntropySet + sum;
        }

        /// <Summary>
        /// Returns the best attribute.
        /// </ Summary>
        /// <Param name = "attributes"> A vector with the attributes </ param>
        /// <Returns> Returns which have higher gain </ returns>
        private Attribute getBestAttribute(DataTable samples, Attribute[] attributes)
        {
            double maxGain = 0.0;
            Attribute result = null;

            foreach (Attribute attribute in attributes)
            {
                double aux = gain(samples, attribute);
                if (aux > maxGain)
                {
                    maxGain = aux;
                    result = attribute;
                }

                    if (maxGain==0)
                    {
                        result = attribute;
                    }
            }
            return result;
        }

        /// <Summary>
        /// Returns true if all examples of sampling are positive
        /// </ Summary>
        /// <Param name = "sample"> DataTable with samples </ param>
        /// <Param name = "targetAttribute"> attribute (column) of the table which will be checked </ param>
        /// <Returns> True if all examples of sampling are positive </ returns>
        private bool allSamplesPositives(DataTable samples, string targetAttribute)
        {
            foreach (DataRow row in samples.Rows)
            {
                if ((bool)row[targetAttribute] == false)
                    return false;
            }

            return true;
        }

        /// <Summary>
        /// Returns true if all examples of sampling are negative
        /// </ Summary>
        /// <Param name = "sample"> DataTable with samples </ param>
        /// <Param name = "targetAttribute"> attribute (column) of the table which will be checked </ param>
        /// <Returns> True if all examples of sampling are negative </ returns>
        private bool allSamplesNegatives(DataTable samples, string targetAttribute)
        {
            foreach (DataRow row in samples.Rows)
            {
                if ((bool)row[targetAttribute] == true)
                    return false;
            }

            return true;
        }

        /// <Summary>
        /// Returns a list of all the distinct values of a sample table
        /// </ Summary>
        /// <Param name = "sample"> DataTable with samples </ param>
        /// <Param name = "targetAttribute"> attribute (column) of the table which will be checked </ param>
        /// <Returns> A ArrayList with the distinct values </ returns>
        private ArrayList getDistinctValues(DataTable samples, string targetAttribute)
        {
            ArrayList distinctValues = new ArrayList(samples.Rows.Count);

            foreach (DataRow row in samples.Rows)
            {
                if (distinctValues.IndexOf(row[targetAttribute]) == -1)
                    distinctValues.Add(row[targetAttribute]);
            }

            return distinctValues;
        }

        /// <Summary>
        /// Returns the most common value within a sampling
        /// </ Summary>
        /// <Param name = "sample"> DataTable with samples </ param>
        /// <Param name = "targetAttribute"> attribute (column) of the table which will be checked </ param>
        /// <Returns> Returns the object with the highest incidence within the sample table </ returns>
        private object getMostCommonValue(DataTable samples, string targetAttribute)
        {
                
                    ArrayList distinctValues = getDistinctValues(samples, targetAttribute);
                    int[] count = new int[distinctValues.Count];
            
            foreach (DataRow row in samples.Rows)
            {
                int index = distinctValues.IndexOf(row[targetAttribute]);
                count[index]++;
            }

            int MaxIndex = 0;
            int MaxCount = 0;

            for (int i = 0; i < count.Length; i++)
            {
                if (count[i] > MaxCount)
                {
                    MaxCount = count[i];
                    MaxIndex = i;
                }
            }

            return distinctValues[MaxIndex];
        }

        /// <Summary>
        /// Monta a decision tree based on the submitted samples
        /// </ Summary>
        /// <Param name = "sample"> table with samples that will be provided for mounting the tree </ param>
        /// <Param name = "targetAttribute"> table column name that possesses the true or false value to
        /// Validate or a sampling </ param>
        /// <Returns> The root of the decision tree mounted </ returns> </ returns?>
        private TreeNode internalMountTree(DataTable samples, string targetAttribute, Attribute[] attributes)
        {
            if (allSamplesPositives(samples, targetAttribute) == true)
                return new TreeNode(new Attribute(true));

            if (allSamplesNegatives(samples, targetAttribute) == true)
                return new TreeNode(new Attribute(false));

            if (attributes.Length == 0)
                return new TreeNode(new Attribute(getMostCommonValue(samples, targetAttribute)));

            mTotal = samples.Rows.Count;
            mTargetAttribute = targetAttribute;
            mTotalPositives = countTotalPositives(samples);

            mEntropySet = calcEntropy(mTotalPositives, mTotal - mTotalPositives);

            Attribute bestAttribute = getBestAttribute(samples, attributes);

            TreeNode root = new TreeNode(bestAttribute);

            DataTable aSample = samples.Clone();

            foreach (string value in bestAttribute.values)
            {
                // Select all the elements with the value of this attribute		
                aSample.Rows.Clear();

                DataRow[] rows = samples.Select(bestAttribute.AttributeName + " = " + "'" + value + "'");

                foreach (DataRow row in rows)
                {
                    aSample.Rows.Add(row.ItemArray);
                }
                // Select all the elements with the value of this attribute

                // Create a new list of attributes less the current attribute that is the best attribute			
                ArrayList aAttributes = new ArrayList(attributes.Length - 1);
                for (int i = 0; i < attributes.Length; i++)
                {
                    if (attributes[i].AttributeName != bestAttribute.AttributeName)
                        aAttributes.Add(attributes[i]);
                }
                // Create a new list of attributes less the current attribute that is the best attribute

                if (aSample.Rows.Count == 0)
                {
                    //return new TreeNode(new Attribute(getMostCommonValue(aSample, targetAttribute)));
                }
                else
                {
                    DecisionTreeID3 dc3 = new DecisionTreeID3();
                    TreeNode ChildNode = dc3.mountTree(aSample, targetAttribute, (Attribute[])aAttributes.ToArray(typeof(Attribute)));
                    root.AddTreeNode(ChildNode, value);
                }
            }

            return root;
        }

        /// <Summary>
        /// Monta a decision tree based on the submitted samples
        /// </ Summary>
        /// <Param name = "sample"> table with samples that will be provided for mounting the tree </ param>
        /// <Param name = "targetAttribute"> table column name that possesses the true or false value to
        /// Validate or a sampling </ param>
        /// <Returns> The root of the decision tree mounted </ returns> </ returns?>
        public TreeNode mountTree(DataTable samples, string targetAttribute, Attribute[] attributes)
        {
            mSamples = samples;
            return internalMountTree(mSamples, targetAttribute, attributes);
        }
    }

    /// <summary>
    /// Class exemplifies the use of ID3
    /// </summary>
    class ID3Sample
    {


    }
    public static void printNode(TreeNode root, string tabs)
    {
            if (root == null)
                return;
        Console.WriteLine(tabs + '|' + root.attribute + '|');

        if (root.attribute.values != null)
        {
            for (int i = 0; i < root.attribute.values.Length; i++)
            {
                Console.WriteLine(tabs + "\t" + "<" + root.attribute.values[i] + ">");
                TreeNode childNode = root.getChildByBranchName(root.attribute.values[i]);
                printNode(childNode, "\t" + tabs);
            }
        }

    }

    static DataTable getDataTable()
    {
        DataTable result = new DataTable("Banck");
        DataColumn column = result.Columns.Add("Age");
        column.DataType = typeof(string);

        column = result.Columns.Add("Marital");
        column.DataType = typeof(string);

            column = result.Columns.Add("Dducation");
            column.DataType = typeof(string);

            column = result.Columns.Add("Salary");
        column.DataType = typeof(string);

        column = result.Columns.Add("Housing");
        column.DataType = typeof(string);

            column = result.Columns.Add("Contact");
            column.DataType = typeof(string);

            column = result.Columns.Add("Loan");
            column.DataType = typeof(bool); 


        //result.Rows.Add(new object[] { "sunny", "high", "high", "no", false }); //D1 sunny high high no F
        //result.Rows.Add(new object[] { "sunny", "high", "high", "yes", false }); //D2 sunny high high yes F
        //result.Rows.Add(new object[] { "overcast", "high", "high", "no", true }); //D3 nebulado high high no T
        //result.Rows.Add(new object[] { "rainy", "high", "high", "no", true }); //D4 rainy high high no T
        //result.Rows.Add(new object[] { "rainy", "low", "normal", "no", true }); //D5 rainy low normal no T
        //result.Rows.Add(new object[] { "rainy", "low", "normal", "yes", false }); //D6 rainy low normal yes F
        //result.Rows.Add(new object[] { "overcast", "low", "normal", "yes", true }); //D7 nebulado low normal yes T
        //result.Rows.Add(new object[] { "sunny", "mild", "high", "no", false }); //D8 sunny mild high no F
        //result.Rows.Add(new object[] { "sunny", "low", "normal", "no", true }); //D9 sunny low normal no T
        //result.Rows.Add(new object[] { "rainy", "mild", "normal", "no", true }); //D10 rainy mild normal no T
        //result.Rows.Add(new object[] { "sunny", "mild", "normal", "no", true }); //D11 sunny mild normal yes T
        //result.Rows.Add(new object[] { "overcast", "mild", "high", "yes", true }); //D12 nebulado mild high yes T
        //result.Rows.Add(new object[] { "overcast", "high", "normal", "no", true }); //D13 nebulado high normal no T
        //result.Rows.Add(new object[] { "rainy", "mild", "high", "yes", false }); //D14 rainy mild high yes F

        return result;

    }

    public static void get_data(DataTable dt )
    {
            string typefile = ".xlsx";
            //string typefile = ".csv";
            string xfile;
        try
        {
            string str1, str2;

            if (typefile == ".xls")
            {
                str1 = "provider=Microsoft.Jet.OLEDB.4.0;Data Source=";
                str2 = ";Extended Properties='Excel 8.0;HDR=YES'"; //;HDR=YES;IMEX=1;MAXSCANROWS=15;READONLY=FALSE
            }
                else if (typefile == ".xlsx")
                {
                 
                    str1 = "Provider= Microsoft.ACE.OLEDB.12.0;Data Source=";
                  str2 = "; Extended Properties=\"Excel 12.0;HDR=Yes\"";

            }
                else if (typefile == ".csv")
                {
                    str1 = "Provider= Microsoft.Jet.OLEDB.4.0;Data Source=";
                    str2 = "; Extended Properties=\"text;HDR=Yes;FMT=Delimited\"";
                }
                else
                {
                    return;
                }

                //Console.WriteLine(dt.Columns.Count);
                System.Data.OleDb.OleDbConnection MyConnection;
            System.Data.DataSet dtset = new DataSet();
            System.Data.OleDb.OleDbDataAdapter MyCommand;
               
            xfile = "E:\\University Part2\\level4 Term2\\Data Mining\\Bank Data.xlsx";
            MyConnection = new System.Data.OleDb.OleDbConnection(str1 + xfile + str2);
            MyCommand = new System.Data.OleDb.OleDbDataAdapter("select * from ["+"Banck Data"+"$A1:G] ", MyConnection);
                //Console.WriteLine(MyCommand.SelectCommand);
                MyCommand.Fill(dt);
            MyConnection.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

    }


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        [STAThread]

        static void Main(string[] args)
        {
            Attribute x1 = new Attribute("Age", new string[] { "child", "young", "old" });
            Attribute x2 = new Attribute("Marital", new string[] { "single", "married" });
            Attribute x3 = new Attribute("Dducation", new string[] { "school", "university", "illiterate" });
            Attribute x4 = new Attribute("Salary", new string[] { "yes", "no" });
            Attribute x5 = new Attribute("Housing", new string[] { "yes", "no" });
            Attribute x6 = new Attribute("Contact", new string[] { "cellular", "telephone" });
            //Attribute x7 = new Attribute("Loan", new bool[] { true,false });


            Attribute[] attributes = new Attribute[] { x1, x2, x3, x4, x5, x6 };

            DataTable samples = getDataTable();
            //DataTable samples = new DataTable();
            get_data(samples);
            Console.WriteLine("Records Number: "+samples.Rows.Count+" Record");
            DecisionTreeID3 id3 = new DecisionTreeID3();
            TreeNode root = id3.mountTree(samples, "Loan", attributes);
            //TreeNode root = id3.mountTree(samples, "Order Quantity", attributes);
            
            printNode(root, "");
            Console.Read();
        }
    }
}
