﻿// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

namespace Microsoft.Azure.Commands.Network
{
    using AutoMapper;
    using Microsoft.Azure.Commands.Network.Models;
    using Microsoft.Azure.Commands.Network.VirtualNetworkGateway;
    using Microsoft.Azure.Commands.ResourceManager.Common.ArgumentCompleters;
    using Microsoft.Azure.Commands.ResourceManager.Common.Tags;
    using Microsoft.Azure.Management.Internal.Resources.Utilities.Models;
    using Microsoft.Azure.Management.Internal.Resources.Utilities.Models;
    using Microsoft.Azure.Management.Network;
    using Microsoft.WindowsAzure.Commands.Common;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using System.Security;
    using System.Security;
    using MNM = Microsoft.Azure.Management.Network.Models;

    namespace Microsoft.Azure.Commands.Network
    {
        [Cmdlet(VerbsCommon.Set,
            "AzureRmVirtualWan",
            DefaultParameterSetName = CortexParameterSetNames.ByVirtualWanName,
            SupportsShouldProcess = true),
            OutputType(typeof(PSVirtualWan))]
        public class SetAzureRmVirtualWanCommand : VirtualWanBaseCmdlet
        {
            [Alias("ResourceName", "VirtualWanName")]
            [Parameter(
                ParameterSetName = CortexParameterSetNames.ByVirtualWanName,
                Mandatory = true,
                HelpMessage = "The virtual wan name.")]
            [ValidateNotNullOrEmpty]
            public virtual string Name { get; set; }

            [Parameter(
                ParameterSetName = CortexParameterSetNames.ByVirtualWanName,
                Mandatory = true,
                ValueFromPipelineByPropertyName = true,
                HelpMessage = "The resource group name.")]
            [ResourceGroupCompleter]
            [ValidateNotNullOrEmpty]
            public virtual string ResourceGroupName { get; set; }

            [Alias("VirtualWan")]
            [Parameter(
                ParameterSetName = CortexParameterSetNames.ByVirtualWanObject,
                Mandatory = true,
                ValueFromPipeline = true,
                HelpMessage = "The virtual wan object to be modified")]
            [ValidateNotNullOrEmpty]
            public PSVirtualWan InputObject { get; set; }

            [Alias("VirtualWanId")]
            [Parameter(
                ParameterSetName = CortexParameterSetNames.ByVirtualWanResourceId,
                Mandatory = true,
                ValueFromPipelineByPropertyName = true,
                HelpMessage = "The Azure resource ID for the virtual wan.")]
            [ResourceIdCompleter("Microsoft.Network/virtualWan")]
            [ValidateNotNullOrEmpty]
            public string ResourceId { get; set; }

            [Parameter(
                Mandatory = false,
                HelpMessage = "Allow vnet to vnet traffic for VirtualWan.")]
            public bool? AllowVnetToVnetTraffic { get; set; }

            [Parameter(
                Mandatory = false,
                HelpMessage = "Allow branch to branch traffic for VirtualWan.")]
            public bool? AllowBranchToBranchTraffic { get; set; }

            [Parameter(
                Mandatory = false,
                HelpMessage = "The list of PSP2SVpnServerConfigurations that are associated with this VirtualWan.")]
            public PSP2SVpnServerConfiguration[] P2SVpnServerConfiguration { get; set; }

            [Parameter(
                Mandatory = false,
                ValueFromPipelineByPropertyName = true,
                HelpMessage = "A hashtable which represents resource tags.")]
            public Hashtable Tag { get; set; }

            [Parameter(
                Mandatory = false,
                HelpMessage = "Do not ask for confirmation if you want to overrite a resource")]
            public SwitchParameter Force { get; set; }

            [Parameter(
                Mandatory = false,
                HelpMessage = "Run cmdlet in the background")]
            public SwitchParameter AsJob { get; set; }

            public override void Execute()
            {
                base.Execute();

                bool shouldProcess = this.Force.IsPresent;
                if (!shouldProcess)
                {
                    shouldProcess = ShouldProcess(this.Name, Properties.Resources.CreatingResourceMessage);
                }

                if (shouldProcess)
                {
                    WriteObject(this.UpdateVirtualWan());
                }
            }

            private PSVirtualWan UpdateVirtualWan()
            {
                if (ParameterSetName.Equals(CortexParameterSetNames.ByVirtualWanObject, StringComparison.OrdinalIgnoreCase))
                {
                    this.Name = this.InputObject.Name;
                    this.ResourceGroupName = this.InputObject.ResourceGroupName;
                }
                else if (ParameterSetName.Equals(CortexParameterSetNames.ByVirtualWanResourceId, StringComparison.OrdinalIgnoreCase))
                {
                    var parsedResourceId = new ResourceIdentifier(this.ResourceId);
                    this.Name = parsedResourceId.ResourceName;
                    this.ResourceGroupName = parsedResourceId.ResourceGroupName;
                }

                //// Let's get the existing VirtualWan - this will throw not found if the VirtualWan does not exist
                var virtualWanToUpdate = GetVirtualWan(this.ResourceGroupName, this.Name);
                if (virtualWanToUpdate == null)
                {
                    throw new PSArgumentException("The VirtualWan to update could not be found");
                }

                if (this.AllowBranchToBranchTraffic.HasValue)
                {
                    virtualWanToUpdate.AllowBranchToBranchTraffic = this.AllowBranchToBranchTraffic.Value;
                }

                if (this.AllowVnetToVnetTraffic.HasValue)
                {
                    virtualWanToUpdate.AllowVnetToVnetTraffic = this.AllowVnetToVnetTraffic.Value;
                }

                // Modify the P2SVpnServerConfigurations if present
                existingVirtualWan.P2SVpnServerConfigurations = new List<PSP2SVpnServerConfiguration>(this.P2SVpnServerConfiguration);

                ConfirmAction(
                    this.Force.IsPresent,
                    string.Format(Properties.Resources.SettingResourceMessage, this.Name),
                    Properties.Resources.SettingResourceMessage,
                    this.Name,
                    () =>
                    {
                        return this.CreateOrUpdateVirtualWan(this.ResourceGroupName, this.Name, existingVirtualWan, this.Tag);
                    });
            }
        }
    }
}