﻿using Opc.UaFx.Client;

namespace Industrialiot.Lib.Data
{
    public class OpcDataChangeHandlerMapper
    {
        public string opcNodeDataName { get; set; }

        public OpcDataChangeReceivedEventHandler handler { get; set; }

        public OpcDataChangeHandlerMapper(string opcNodeDataName, OpcDataChangeReceivedEventHandler handler)
        {
            this.opcNodeDataName = opcNodeDataName;
            this.handler = handler;
        }
    }
}
