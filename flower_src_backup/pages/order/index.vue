<template>
	<view>
		<up-sticky>
			<!-- <up-subsection :list="subsectionList" :current="current" activeColor="#8C4FFF"
				@change="subsectionChange"></up-subsection> -->
			<view class="searchBox">
				<up-search bgColor="#fff" borderColor="#eee" v-model="query.queryName" placeholder="搜索订单"
					:clearabled="false" :showAction="false" @search="getOrderList(true)"></up-search>
			</view>
			<view class="tabs" v-if="current == 1">
				<up-tabs :list="tabsList" :current='tagCurrent' @change="clickTabs" lineColor='#8C4FFF '
					:activeStyle='{ color: "#8C4FFF " }'></up-tabs>
			</view>
		</up-sticky>
		<view class="list" v-if="orderList.length > 0">
			<view class="square" v-for="item in orderList" :key="item.Id">
				<view class="top">
					<view class="left">{{ item.orderNo }}</view>
					<view class="right">{{ item.statusName }}</view>
				</view>
				<view class="item" @click="toDetail(item)">
					<view>
						<image :src="item.goodsImgList[0]" mode="aspectFill"></image>
					</view>
					<view class="texts">
						<view class="title">{{ item.relatedDemand }}</view>
					</view>
				</view>
				<view class="time">
					<view>
						<image src="/static/icon04.png"></image>
					</view>
					<view class="text">收货时间：{{ item.receivingTime }}</view>
				</view>
				<view class="btnBox">
					<view class="prices">
						<view class="text"> 金额: </view>
						<view class="price">￥{{ item.price }}</view>
					</view>
					<view class="btns">
						<view class="btn black" v-if="[0].includes(item.status)"
							@click="clickCancel(item.orderNo)">取消申请
						</view>
						<view class="btn red" v-if="[1].includes(item.status)"
							@click="href('/pages/order/deliver?id=' + item.orderId)">发货
						</view>
					</view>
				</view>
			</view>
			<view style="height: 24rpx;"></view>
		</view>
		<view class="empty" v-else>
			<image src="/static/img32.png" mode="widthFix"></image>
		</view>
	</view>
</template>

<script>
export default {
	data() {
		return {
			current: 1,
			tagCurrent: 0,
			subsectionList: ['我的申请', '我的订单'],
			tabsList: [
				{ name: '全部', key: '', badge: { value: 0 } },
				{ name: '已申请', key: 0, badge: { value: 0 } },
				{ name: '进行中', key: 1, badge: { value: 0 } },
				{ name: '待确认', key: 2, badge: { value: 0 } },
				{ name: '已完成', key: 3, badge: { value: 0 } },
				{ name: '售后完成', key: 4, badge: { value: 0 } },
			],
			query: {
				pageNum: 1,
				pageSize: 10,
				queryState: "",
				startTime: "",
				endTime: "",
				queryName: ""
			},
			total: 0,
			orderList: []
		};
	},
	onLoad(options) {
		this.tagCurrent = options.index || 0
		this.query.queryState = this.tabsList[this.tagCurrent].key
	},
	onShow() {
		this.getOrderList(true)
	},
	onPullDownRefresh() {
		this.getOrderList(true).then(() => {
			uni.stopPullDownRefresh()
		})
	},
	async onReachBottom() {
		if (this.total > this.query.pageNum * this.query.pageSize) {
			this.query.pageNum += 1
			await this.getOrderList()
		}
	},
	methods: {
		subsectionChange(e) {
			this.current = e
			this.query.queryState = this.current == 0 ? 2 : ''
			this.tagCurrent = 0
			this.getOrderList(true)
		},
		// 点击tabs
		clickTabs(e) {
			this.query.queryState = e.key
			this.getOrderList(true)
		},
		async orderCount() {
			let res = await this.request('Order/OrderCount')
			let arr = Object.values(res.data)
			arr.forEach((item, index) => {
				this.tabsList[index + 1].badge.value = item
			})
		},
		async getOrderList(reset) {
			if (!this.getToken()) {
				return
			}
			if (reset) this.query.pageNum = 1
			let res = await this.request('Order/TaskOrderList', this.query)
			// this.orderCount()
			if (this.query.pageNum == 1) {
				this.orderList = res.data.items
				this.total = res.data.total
			} else {
				this.orderList.push(...res.data.items)
			}
		},
		clickCancel(orderNo) {
			this.modal('提示', '确定取消申请吗？', callback => {
				if (callback) {
					this.request('Order/CancelTaskApply', {
						orderNo: orderNo
					}).then(res => {
						this.toast("取消成功")
						this.getOrderList(true)
					})
				}
			})
		},
		toDetail(item) {
			this.href('/pages/order/detail?id=' + item.orderId)
		},
	}
}
</script>

<style lang="scss" scoped>
.searchBox {
	padding: 32rpx;
	padding-bottom: 0;
	position: relative;
	width: 100%;
	display: flex;
	background: white;
}

.tabs {
	padding: 0 32rpx;
	background: white;
}

.evaluate {
	display: flex;
	justify-content: space-between;
	align-items: center;
	margin-top: 24rpx;
	width: 686rpx;
	margin: 0 auto;
	background: #FFFFFF;
	border-radius: 32rpx;
	padding: 16rpx 32rpx;
	margin-top: 24rpx;

	.evaluateLeft {
		display: flex;
		align-items: center;

		image {
			width: 32rpx;
			height: 32rpx;
		}

		.text {
			margin-left: 32rpx;
		}
	}

	.evaluateRight {}
}

.list {
	padding: 0 32rpx;

	.square {
		margin-top: 24rpx;
		width: 686rpx;
		background: #FFFFFF;
		border-radius: 32rpx;
		padding: 16rpx 32rpx;

		.top {
			display: flex;
			padding-top: 14rpx;
			padding-bottom: 29rpx;
			display: flex;
			justify-content: space-between;
			align-items: center;
			border-bottom: 1px solid #EFEFEF;

			.left {
				font-size: 30rpx;
				color: #000000;
				padding-right: 10rpx;
				overflow: hidden;
				text-overflow: ellipsis;
				display: -webkit-box;
				-webkit-box-orient: vertical;
				-webkit-line-clamp: 1;
			}

			.right {
				font-size: 30rpx;
				color: #8C4FFF;
			}
		}

		.item {
			padding: 24rpx;
			border-bottom: 1px solid #EFEFEF;
			display: flex;


			image {
				border-radius: 8rpx;
				width: 132rpx;
				height: 132rpx;
			}

			.texts {
				margin-left: 16rpx;

				.title {
					font-size: 28rpx;
					color: #000000;
					overflow: hidden;
					text-overflow: ellipsis;
					display: -webkit-box;
					-webkit-box-orient: vertical;
					-webkit-line-clamp: 4;
				}

				.content {
					margin-top: 16rpx;
					font-size: 24rpx;
					color: #666666;
				}
			}
		}

		.time {
			display: flex;
			align-items: center;
			padding: 14rpx 18rpx;
			background: linear-gradient(90deg, #F5F5F5 70%, rgba(245, 245, 245, 0) 100%);

			image {
				width: 32rpx;
				height: 32rpx;
			}

			.text {
				font-size: 24rpx;
				color: #666666;
				margin-left: 6rpx;
			}
		}

		.btnBox {
			display: flex;
			align-items: center;
			justify-content: space-between;
			padding: 16rpx 0;

			.prices {
				display: flex;
				align-items: center;

				.text {
					font-size: 28rpx;
					color: #333333;
				}

				.price {
					margin-left: 12rpx;
					font-weight: 600;
					font-size: 32rpx;
					color: #DE4343;
				}
			}

			.btns {
				display: flex;
				align-items: center;
				justify-content: flex-end;
				margin-top: 10rpx;

				.btn {
					width: 152rpx;
					height: 58rpx;
					line-height: 58rpx;
					text-align: center;
					border-radius: 16rpx;
					font-size: 26rpx;
					margin-left: 32rpx;
				}

				.red {
					background: #8C4FFF;
					color: #FFFFFF;
				}

				.black {
					border: 2rpx solid #000000;
					background: #FFFFFF;
					color: #000000;
				}
			}
		}
	}

}

.defaultPage {
	display: flex;
	justify-content: center;
	margin-top: 173rpx;

	image {
		width: 330rpx;
		height: 312rpx;
	}
}
</style>