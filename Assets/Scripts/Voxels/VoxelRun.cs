using Unity.Collections;

//Represent the volume of chunks using runs of orig instead of an array.
//A linked list.
public class VoxelRun
{
    private VoxelRun next;
    public int runLength; //Always at least 1.
    public Voxel type;

    public VoxelRun(int size, int height)
    {
        next = null;
        type = new Voxel(VoxelType.AIR);
        runLength = size * height * size;
    }
    private VoxelRun(Voxel type, int length)
    {
        next = null;
        runLength = length;
        this.type = type;
    }
    /// <summary>
    /// Get a voxel from this list at given index
    /// </summary>
    /// <param name="head">The head of the list</param>
    /// <returns>The voxel at the requested index</returns>
    public static Voxel Get(VoxelRun head, int index)
    {
        return FindRun(head, ref index).type;
    }
    /// <summary>
    /// Finds the run that contains the index.
    /// </summary>
    /// <param name="head"></param>
    /// <param name="index"></param>
    /// <exception cref="System.Exception"></exception>
    private static VoxelRun FindRun(VoxelRun head, ref int index)
    {
        while (head != null && index >= head.runLength)
        {
            index -= head.runLength;
            head = head.next;
        }

        //Either we're in the correct run or we ran off the end of the list
        if (head == null)
        {
            throw new System.Exception("Given index out of bounds of the list.");
        }
        return head;
    }
    /// <summary>
    /// Set the orig at the index and for the length to the given voxel.
    /// </summary>
    /// <param name="head">The head of the list</param>
    /// <param name="type">The voxel type to insert</param>
    /// <param name="index">The starting index</param>
    /// <param name="length">The run length</param>
    /// <returns>True if the list was modified, false otherwise</returns>
    public static bool Set(VoxelRun head, Voxel type, int index, int length=1)
    {
        //This is implicitly a bounds check. An error will be thrown if out of range.
        VoxelRun start = FindRun(head, ref index);
        int lastIndex = index + length-1;
        VoxelRun end = FindRun(start, ref lastIndex);

        bool fromBeginning = index == 0;
        bool toEnd = lastIndex == end.runLength - 1;

        //if the run encapsulates the desired insertion then it doesn't change
        if (Voxel.Equals(start.type, type) && start == end)
        {
            return false;
        }

        //I could not find an elegant way to handle this, so we enumerate each possiblity
        if (start == end) //scenarios 1,2,3,4
        {
            if (!fromBeginning && !toEnd)//scenario 1
            {
                VoxelRun mid1 = new VoxelRun(type, length);
                VoxelRun mid2 = new VoxelRun(start.type, start.runLength - (lastIndex + 1));
                start.runLength = index;

                mid2.next = start.next;
                mid1.next = mid2;
                start.next = mid1;
            }
            else if (fromBeginning && !toEnd)//scenario 2
            {
                VoxelRun mid = new VoxelRun(start.type, start.runLength - length);
                start.runLength = length;
                start.type = type;

                mid.next = start.next;
                start.next = mid;
            }
            else if (!fromBeginning && toEnd)//scenario 3
            {
                VoxelRun mid = new VoxelRun(type, length);
                start.runLength -= length;

                mid.next = start.next;
                start.next = mid;
            }
            else if (fromBeginning && toEnd)//scenario 4
            {
                start.type = type;
            }
        }
        else //scenarios 5,6,7,8
        {
            if (!fromBeginning && !toEnd)//scenario 7
            {
                VoxelRun mid = new VoxelRun(type, length);
                start.runLength = index;
                end.runLength -= lastIndex + 1;

                mid.next = end;
                start.next = mid;
            }
            else if (fromBeginning && !toEnd)//scenario 5
            {
                start.runLength = length;
                end.runLength -= lastIndex + 1;
                start.type = type;

                start.next = end;
            }
            else if (!fromBeginning && toEnd)//scenario 8
            {
                start.runLength = index;
                end.runLength = length;
                end.type = type;

                start.next = end;
            }
            else if (fromBeginning && toEnd)//scenario 6
            {
                start.type = type;
                start.runLength = length;
                start.next = end.next;
            }
        }

        VoxelRun.merge(head); // to clean up possible equivalent neighbors
        return true;
    }
    //Merge adjacent runs if they contain the same voxel
    private static void merge(VoxelRun head)
    {
        while (head.next != null)
        {
            if (Voxel.Equals(head.type, head.next.type))
            {
                head.runLength += head.next.runLength;
                head.next = head.next.next;
            }
            else
            {
                head = head.next;
            }
        }
    }
    public static int GetSize(VoxelRun head)
    {
        int size = 0;
        while (head != null)
        {
            size += head.runLength;
            head = head.next;
        }
        return size;
    }

    /// <summary>
    /// Used after a job completes.
    /// </summary>
    /// <param name="voxels"></param>
    /// <returns></returns>
    public static VoxelRun toVoxelRun(NativeArray<Voxel> voxels)
    {
        VoxelRun head = new VoxelRun(voxels[0], 1);
        VoxelRun current = head;
        for (int i = 1; i < voxels.Length; i++)
        {
            if (Voxel.Equals(current.type, voxels[i]))
            {
                current.runLength++;
            }
            else
            {
                VoxelRun newRun = new(voxels[i], 1);
                current.next = newRun;
                current = newRun;
            }
        }
        return head;
    }

    /// <summary>
    /// Used by the toNativeList function to convert this VoxelRun to something
    /// usable by a job.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="T2"></typeparam>
    public struct Pair<T, T2>
    {
        public T key;
        public T2 value;
        public Pair(T key, T2 value)
        {
            this.key = key;
            this.value = value;
        }
    }

    /// <summary>
    /// Turns the VoxelRun data into something you can pass into a job.
    /// MAKE SURE YOU DISPOSE WHEN FINISHED!
    /// </summary>
    public static NativeList<Pair<Voxel, int>> ToNativeList(VoxelRun head, int size, int height)
    {
        if (head == null)
        {
            head = new VoxelRun(size, height);
        }
        NativeList<Pair<Voxel, int>> newList = new(Allocator.TempJob);

        while (head != null)
        {
            newList.Add(new(head.type, head.runLength));
            head = head.next;
        }
        return newList;
    }
}
