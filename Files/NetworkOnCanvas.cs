using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using AI_labs.Network;

namespace AI_labs.Files;

[DataContract]
[KnownType(typeof(NetworkOnCanvas))]
public class NetworkOnCanvas: IExtensibleDataObject
{
    [DataMember] public readonly Point[] NodePositions;
    [DataMember] public readonly Path[] Paths;
    
    [DataContract]
    public struct Path
    {
        [DataMember] public readonly int NodeA;
        [DataMember] public readonly int NodeB;
        [DataMember] public readonly int Length;

        public Path(int nodeA, int nodeB, int length)
        {
            NodeA = nodeA;
            NodeB = nodeB;
            Length = length;
        }
    }

    public NetworkOnCanvas(Network<int> network, IReadOnlyList<Ellipse> nodes)
    {
        var paths = new List<Path>();
        network.ForEachPath((a, b, length) =>
        {
            paths.Add(new Path(a, b, length));
            return null;
        });
        Paths = paths.ToArray();
        NodePositions = new Point[nodes.Count];
        for (var i = 0; i < nodes.Count; i++)
        {
            NodePositions[i] = new Point(
                Canvas.GetLeft(nodes[i]),
                Canvas.GetTop(nodes[i])
            );
        }
    }

    public void SaveToFile(string filepath)
    {
        using var stream = File.Open(filepath, FileMode.Create);
        var serializer = new DataContractSerializer(typeof(NetworkOnCanvas));
        serializer.WriteObject(stream, this);
    }

    public static NetworkOnCanvas? ReadFromFile(string filepath)
    {
        using var stream = File.Open(filepath, FileMode.Open);
        var serializer = new DataContractSerializer(typeof(NetworkOnCanvas));
        return serializer.ReadObject(stream) as NetworkOnCanvas;
    }

    public ExtensionDataObject? ExtensionData { get; set; }
}