# UnityNeuralNetworkVisualizer

A GPU accelerated visuallizer of a neural network structure.

Compute shaders are used to create a custom render pipeline for neural network nodes,
which test nodes per pixel of resolution. This makes the performance tied only to
the window size, not the neural network size, allowing the rendering of networks of
hundreds of thousands of nodes in realtime.
